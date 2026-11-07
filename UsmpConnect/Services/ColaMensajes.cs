using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UsmpConnect.Data;
using UsmpConnect.Models;

namespace UsmpConnect.Services;

public record MensajeCola(string Tipo, string Contenido, DateTime Timestamp);

public record EventoResenaCola(
    int ResenaId,
    int RestauranteId,
    string RestauranteNombre,
    string UsuarioId,
    string UsuarioNombre,
    int Puntuacion,
    double NuevaCalificacion,
    int TotalResenas,
    DateTime FechaUtc);

public interface IColaMensajes
{
    Task PublicarAsync<T>(string cola, T mensaje);
    bool IsConnected { get; }
    string Estado { get; }
}

public class CloudAmqpColaMensajes : IColaMensajes, IAsyncDisposable, IDisposable
{
    public const string ColaPorDefecto = "usmp_notificaciones_cola";

    private readonly ILogger<CloudAmqpColaMensajes> _logger;
    private readonly EstadoIntegraciones? _estados;
    private readonly string? _amqpUrl;
    private readonly string _colaPorDefecto;
    private readonly Channel<MensajeCola> _colaLocal = Channel.CreateUnbounded<MensajeCola>();
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _publishChannel;
    private bool _disposed;

    public bool IsConnected => _publishChannel?.IsOpen ?? false;
    public string Estado => IsConnected ? "CloudAMQP RabbitMQ (Conectado)" : "Cola Asíncrona Local (Fallback)";
    public ChannelReader<MensajeCola> LecturaLocal => _colaLocal.Reader;
    public bool TieneConfiguracionRemota => !string.IsNullOrWhiteSpace(_amqpUrl);

    public CloudAmqpColaMensajes(IConfiguration config, ILogger<CloudAmqpColaMensajes> logger, EstadoIntegraciones? estados = null)
    {
        _logger = logger;
        _estados = estados;
        _amqpUrl = config["CloudAMQP:Url"];
        _colaPorDefecto = string.IsNullOrWhiteSpace(config["CloudAMQP:QueueName"])
            ? ColaPorDefecto
            : config["CloudAMQP:QueueName"]!;
    }

    public async Task<bool> AsegurarConexionAsync(string? cola = null, CancellationToken ct = default)
    {
        if (_disposed || string.IsNullOrWhiteSpace(_amqpUrl))
            return false;

        if (_publishChannel is { IsOpen: true })
            return true;

        await _connectLock.WaitAsync(ct);
        try
        {
            if (_publishChannel is { IsOpen: true })
                return true;

            var factory = new ConnectionFactory
            {
                Uri = new Uri(_amqpUrl),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _publishChannel?.Dispose();
            if (_connection is not { IsOpen: true })
            {
                _connection?.Dispose();
                _connection = await factory.CreateConnectionAsync(ct);
            }
            _publishChannel = await _connection.CreateChannelAsync(cancellationToken: ct);

            var nombreCola = string.IsNullOrWhiteSpace(cola) ? _colaPorDefecto : cola;
            await DeclararColaAsync(_publishChannel, nombreCola, ct);

            _logger.LogInformation("CloudAMQP (RabbitMQ) conectado exitosamente a la cola {Cola}.", nombreCola);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo conectar a CloudAMQP. Usando cola en memoria local.");
            return false;
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async Task<IChannel?> CrearCanalConsumidorAsync(string cola, CancellationToken ct = default)
    {
        if (!await AsegurarConexionAsync(cola, ct) || _connection is not { IsOpen: true })
            return null;

        try
        {
            var consumerChannel = await _connection.CreateChannelAsync(cancellationToken: ct);
            await DeclararColaAsync(consumerChannel, cola, ct);
            await consumerChannel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: ct);
            return consumerChannel;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo abrir canal consumidor de CloudAMQP para la cola {Cola}.", cola);
            return null;
        }
    }

    private static Task DeclararColaAsync(IChannel channel, string cola, CancellationToken ct) =>
        channel.QueueDeclareAsync(
            queue: cola,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

    public async Task PublicarAsync<T>(string cola, T mensaje)
    {
        var nombreCola = string.IsNullOrWhiteSpace(cola) ? _colaPorDefecto : cola;
        var json = JsonSerializer.Serialize(mensaje);
        var envelope = new MensajeCola(typeof(T).Name, json, DateTime.UtcNow);

        if (await AsegurarConexionAsync(nombreCola) && _publishChannel is { IsOpen: true })
        {
            try
            {
                await _connectLock.WaitAsync();
                try
                {
                    await DeclararColaAsync(_publishChannel, nombreCola, CancellationToken.None);
                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
                    var props = new BasicProperties { Persistent = true };
                    await _publishChannel.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: nombreCola,
                        mandatory: false,
                        basicProperties: props,
                        body: body);
                }
                finally
                {
                    _connectLock.Release();
                }

                _estados?.Registrar("rabbit-publicacion", true, "Evento enviado por el productor a RabbitMQ remoto");
                _logger.LogInformation("Mensaje publicado en CloudAMQP ({Cola}): {Tipo}", nombreCola, typeof(T).Name);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo al publicar en CloudAMQP. Encolando en memoria local.");
            }
        }

        _estados?.Registrar("rabbit-publicacion", false, "Evento encolado en el respaldo local");
        await _colaLocal.Writer.WriteAsync(envelope);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        _colaLocal.Writer.TryComplete();

        try
        {
            if (_publishChannel is not null)
                await _publishChannel.CloseAsync();
        }
        catch { }

        try
        {
            if (_connection is not null)
                await _connection.CloseAsync();
        }
        catch { }

        _publishChannel?.Dispose();
        _connection?.Dispose();
        _connectLock.Dispose();
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}

public class ConsumidorNotificacionesService : BackgroundService
{
    private readonly IColaMensajes _colaMensajes;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<ConsumidorNotificacionesService> _logger;
    private readonly EstadoIntegraciones? _estados;
    private IChannel? _consumerChannel;

    public ConsumidorNotificacionesService(
        IColaMensajes colaMensajes,
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<ConsumidorNotificacionesService> logger, EstadoIntegraciones? estados = null)
    {
        _colaMensajes = colaMensajes;
        _estados = estados;
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var nombreCola = string.IsNullOrWhiteSpace(_config["CloudAMQP:QueueName"])
            ? CloudAmqpColaMensajes.ColaPorDefecto
            : _config["CloudAMQP:QueueName"]!;

        if (_colaMensajes is CloudAmqpColaMensajes cloudCola)
        {
            await Task.WhenAll(
                MantenerConsumidorRabbitAsync(cloudCola, nombreCola, stoppingToken),
                ConsumirLocalAsync(cloudCola, stoppingToken));
        }
    }

    private async Task MantenerConsumidorRabbitAsync(CloudAmqpColaMensajes cloudCola, string nombreCola, CancellationToken ct)
    {
        if (!cloudCola.TieneConfiguracionRemota) return;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (_consumerChannel is not { IsOpen: true })
                {
                    _consumerChannel?.Dispose();
                    _consumerChannel = null;
                    await IniciarConsumidorRabbitAsync(cloudCola, nombreCola, ct);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _consumerChannel?.Dispose();
                _consumerChannel = null;
                _estados?.Registrar("rabbit-consumo", false, "Suscripción remota fallida; pendiente de reintento");
                _logger.LogWarning(ex, "Se reintentará la suscripción a RabbitMQ.");
            }
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
        }
    }

    private async Task ConsumirLocalAsync(CloudAmqpColaMensajes cloudCola, CancellationToken stoppingToken)
    {
        await foreach (var envelope in cloudCola.LecturaLocal.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcesarSobreAsync(envelope, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar mensaje de la cola local ({Tipo}).", envelope.Tipo);
            }
        }
    }

    private async Task IniciarConsumidorRabbitAsync(CloudAmqpColaMensajes cloudCola, string nombreCola, CancellationToken stoppingToken)
    {
        _consumerChannel = await cloudCola.CrearCanalConsumidorAsync(nombreCola, stoppingToken);
        if (_consumerChannel is null)
            return;

        var channel = _consumerChannel;
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            if (channel is not { IsOpen: true })
                return;

            MensajeCola? envelope;
            try
            {
                var raw = Encoding.UTF8.GetString(ea.Body.ToArray());
                envelope = JsonSerializer.Deserialize<MensajeCola>(raw);
                if (envelope is null || string.IsNullOrWhiteSpace(envelope.Tipo))
                {
                    _estados?.Registrar("rabbit-consumo", false, "Mensaje remoto inválido descartado");
                    _logger.LogWarning("Mensaje inválido en cola {Cola}. Se descarta sin reencolar.", nombreCola);
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, stoppingToken);
                    return;
                }
            }
            catch (JsonException ex)
            {
                _estados?.Registrar("rabbit-consumo", false, "JSON remoto inválido descartado");
                _logger.LogWarning(ex, "JSON corrupto recibido en cola {Cola}. Se descarta sin reencolar.", nombreCola);
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, stoppingToken);
                return;
            }

            try
            {
                await ProcesarSobreAsync(envelope, stoppingToken);
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
                _estados?.Registrar("rabbit-consumo", true, "Evento consumido desde RabbitMQ remoto y ACK enviado");
            }
            catch (JsonException ex)
            {
                _estados?.Registrar("rabbit-consumo", false, "Carga remota inválida descartada");
                _logger.LogWarning(ex, "Carga útil inválida para el tipo {Tipo}. Se descarta sin reencolar.", envelope.Tipo);
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            }
            catch (Exception ex)
            {
                _estados?.Registrar("rabbit-consumo", false, "Procesamiento remoto fallido");
                var reencolar = !ea.Redelivered;
                _logger.LogError(ex, "Fallo al procesar mensaje {Tipo} en RabbitMQ (requeue={Requeue}).", envelope.Tipo, reencolar);
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: reencolar, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: nombreCola,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Consumidor asíncrono suscrito a la cola RabbitMQ {Cola}.", nombreCola);
    }

    public async Task ProcesarSobreAsync(MensajeCola envelope, CancellationToken ct = default)
    {
        if (envelope.Tipo != nameof(EventoResenaCola))
        {
            _logger.LogInformation("Mensaje de cola recibido ({Tipo}) sin procesador específico.", envelope.Tipo);
            return;
        }

        var evento = JsonSerializer.Deserialize<EventoResenaCola>(envelope.Contenido)
            ?? throw new JsonException("No se pudo deserializar EventoResenaCola.");

        var minDelay = Math.Max(0, _config.GetValue("CloudAMQP:DelayMinSeconds", 1));
        var maxDelay = Math.Max(minDelay, _config.GetValue("CloudAMQP:DelayMaxSeconds", 2));
        if (maxDelay > 0)
        {
            var delayMs = Random.Shared.Next(minDelay * 1000, maxDelay * 1000 + 1);
            if (delayMs > 0)
                await Task.Delay(delayMs, ct);
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var pieHost = scope.ServiceProvider.GetRequiredService<IPieHostService>();

        var mensajeNoti = $"Tu reseña #{evento.ResenaId} de {evento.RestauranteNombre} ({evento.Puntuacion} ★) fue procesada y publicada.";

        var persistida = false;
        if (!string.IsNullOrWhiteSpace(evento.UsuarioId))
        {
            var existeUsuario = await db.Users.AsNoTracking().AnyAsync(u => u.Id == evento.UsuarioId, ct);
            if (existeUsuario)
            {
                var yaExiste = await db.Notificaciones
                    .AsNoTracking()
                    .AnyAsync(n => n.UsuarioId == evento.UsuarioId && n.Mensaje == mensajeNoti, ct);

                if (!yaExiste)
                {
                    db.Notificaciones.Add(new Notificacion
                    {
                        UsuarioId = evento.UsuarioId,
                        Mensaje = mensajeNoti,
                        Url = "/Restaurantes",
                        Icono = "star-fill",
                        Leida = false,
                        Fecha = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync(ct);
                }
                persistida = true;
            }
        }

        _estados?.Registrar("notificacion", persistida, persistida ? "Notificación guardada o ya existente (idempotente)" : "No se guardó notificación: destinatario inexistente");
        await pieHost.PublicarEventoAsync("1", "nueva_resena", new
        {
            RestauranteId = evento.RestauranteId,
            RestauranteNombre = evento.RestauranteNombre,
            NuevaCalificacion = evento.NuevaCalificacion,
            TotalResenas = evento.TotalResenas,
            Usuario = evento.UsuarioNombre,
            Puntuacion = evento.Puntuacion
        });
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        try
        {
            if (_consumerChannel is { IsOpen: true })
                await _consumerChannel.CloseAsync(cancellationToken);
        }
        catch { }

        _consumerChannel?.Dispose();
    }
}
