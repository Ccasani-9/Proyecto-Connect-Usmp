using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace UsmpConnect.Services;

public record MensajeCola(string Tipo, string Contenido, DateTime Timestamp);

public interface IColaMensajes
{
    Task PublicarAsync<T>(string cola, T mensaje);
    bool IsConnected { get; }
    string Estado { get; }
}

public class CloudAmqpColaMensajes : IColaMensajes
{
    private readonly ILogger<CloudAmqpColaMensajes> _logger;
    private readonly string? _amqpUrl;
    private readonly Channel<MensajeCola> _colaLocal = Channel.CreateUnbounded<MensajeCola>();
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly object _lock = new();

    public bool IsConnected => _channel?.IsOpen ?? false;
    public string Estado => IsConnected ? "CloudAMQP RabbitMQ (Conectado)" : "Cola Asíncrona Local (Fallback)";

    public CloudAmqpColaMensajes(IConfiguration config, ILogger<CloudAmqpColaMensajes> logger)
    {
        _logger = logger;
        _amqpUrl = config["CloudAMQP:Url"];
        _ = ConectarAsync();
    }

    private async Task ConectarAsync()
    {
        if (string.IsNullOrWhiteSpace(_amqpUrl))
        {
            _logger.LogInformation("CloudAMQP URL no configurada. Usando cola en memoria.");
            return;
        }

        try
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_amqpUrl),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: "usmp_notificaciones_cola",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation("CloudAMQP (RabbitMQ) conectado exitosamente a la cola usmp_notificaciones_cola.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo conectar a CloudAMQP. Usando cola en memoria local.");
        }
    }

    public async Task PublicarAsync<T>(string cola, T mensaje)
    {
        var json = JsonSerializer.Serialize(mensaje);
        var envelope = new MensajeCola(typeof(T).Name, json, DateTime.UtcNow);

        if (IsConnected && _channel != null)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
                await _channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: cola,
                    body: body);

                _logger.LogInformation("Mensaje publicado en CloudAMQP ({Cola}): {Tipo}", cola, typeof(T).Name);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo al publicar en CloudAMQP. Encolando en memoria local.");
            }
        }

        // Fallback local
        await _colaLocal.Writer.WriteAsync(envelope);
    }
}
