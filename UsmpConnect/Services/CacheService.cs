using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace UsmpConnect.Services;

public record LecturaCache<T>(T? Valor, string Origen);

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key);
    async Task<LecturaCache<T>> LeerConOrigenAsync<T>(string key)
        => new(await GetAsync<T>(key), IsConnected ? "Redis Cache" : "Caché local");
    Task SetAsync<T>(string key, T data, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    bool IsConnected { get; }
    string Estado { get; }
}

public sealed class RedisCacheService : IRedisCacheService, IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly EstadoIntegraciones? _estados;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IConnectionMultiplexer? _redis;
    private readonly IDatabase? _db;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly HashSet<string> _invalidacionesPendientes = [];

    public bool IsConnected => _redis?.IsConnected ?? false;
    public string Estado => IsConnected ? "Redis Cloud (Conectado)" : "Caché en Memoria (Fallback)";

    public RedisCacheService(IConfiguration config, IMemoryCache memoryCache, ILogger<RedisCacheService> logger, EstadoIntegraciones? estados = null)
    {
        _memoryCache = memoryCache;
        _estados = estados;
        _logger = logger;
        var connStr = config["Redis:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connStr)) return;
        try
        {
            var options = ConfigurationOptions.Parse(connStr);
            options.AbortOnConnectFail = false;
            options.ConnectTimeout = 4000;
            options.SyncTimeout = 4000;
            options.AsyncTimeout = 4000;
            // No acumular escrituras antiguas para reproducirlas después de una caída.
            options.BacklogPolicy = BacklogPolicy.FailFast;
            _redis = ConnectionMultiplexer.Connect(options);
            _db = _redis.GetDatabase();
            _logger.LogInformation("Caché inicializada: {Estado}", Estado);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo inicializar Redis. Usando MemoryCache.");
        }
    }

    public async Task<T?> GetAsync<T>(string key) => (await LeerConOrigenAsync<T>(key)).Valor;

    public async Task<LecturaCache<T>> LeerConOrigenAsync<T>(string key)
    {
        var fullKey = $"usmp:{key}";
        await _gate.WaitAsync();
        try
        {
            if (IsConnected && _db is not null)
            {
                try
                {
                    if (_invalidacionesPendientes.Contains(fullKey))
                    {
                        await _db.KeyDeleteAsync(fullKey);
                        _invalidacionesPendientes.Remove(fullKey);
                    }
                    var val = await _db.StringGetAsync(fullKey);
                    if (val.HasValue)
                    {
                        var valor = JsonSerializer.Deserialize<T>((string)val!);
                        if (valor is null) throw new JsonException("Entrada nula en caché");
                        _estados?.Registrar("redis", true, "Lectura del catálogo atendida por Redis remoto");
                        return new(valor, "Redis Cache");
                    }
                    // Otra instancia pudo invalidar o la clave pudo expirar.
                    // Una ausencia confirmada en Redis no debe revivir la copia local anterior.
                    _memoryCache.Remove(fullKey);
                    _estados?.Registrar("redis", false, "Redis respondió sin copia; catálogo se reconstruye desde SQLite");
                    return new(default, "SQLite");
                }
                catch (JsonException ex)
                {
                    _memoryCache.Remove(fullKey);
                    _invalidacionesPendientes.Add(fullKey);
                    _estados?.Registrar("redis", false, "Entrada remota inválida; catálogo se reconstruye desde SQLite");
                    _logger.LogWarning(ex, "Entrada de caché inválida ({Key}); se recargará desde SQLite", fullKey);
                    return new(default, "SQLite");
                }
                catch (RedisException ex)
                {
                    _logger.LogWarning(ex, "Error al leer de Redis ({Key}). Usando respaldo local.", fullKey);
                }
            }
            var encontrada = _memoryCache.TryGetValue(fullKey, out T? localVal) && localVal is not null;
            _estados?.Registrar("redis", false, encontrada ? "Lectura mediante respaldo en memoria local" : "Sin copia en caché; catálogo se reconstruye desde SQLite");
            return new(encontrada ? localVal : default, encontrada ? "Caché local" : "SQLite");
        }
        finally { _gate.Release(); }
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan? expiry = null)
    {
        var fullKey = $"usmp:{key}";
        var json = JsonSerializer.Serialize(data);
        var tiempo = expiry ?? TimeSpan.FromMinutes(10);
        await _gate.WaitAsync();
        try
        {
            if (_db is not null)
            {
                // Si falla la escritura, borrar la copia remota anterior al recuperar conexión.
                _invalidacionesPendientes.Add(fullKey);
                if (IsConnected)
                {
                    try
                    {
                        await _db.StringSetAsync(fullKey, json, tiempo);
                        _invalidacionesPendientes.Remove(fullKey);
                    }
                    catch (RedisException ex)
                    {
                        _logger.LogWarning(ex, "Error al escribir en Redis ({Key}). Usando respaldo local.", fullKey);
                    }
                }
            }
            _memoryCache.Set(fullKey, data, tiempo);
        }
        finally { _gate.Release(); }
    }

    public async Task RemoveAsync(string key)
    {
        var fullKey = $"usmp:{key}";
        await _gate.WaitAsync();
        try
        {
            _memoryCache.Remove(fullKey);
            _estados?.Registrar("redis-invalidacion", false, "Copia local eliminada; invalidación remota no completada");
            if (_db is null) return;
            _invalidacionesPendientes.Add(fullKey);
            if (IsConnected)
            {
                try
                {
                    await _db.KeyDeleteAsync(fullKey);
                    _invalidacionesPendientes.Remove(fullKey);
                    _estados?.Registrar("redis-invalidacion", true, "Invalidación remota completada");
                }
                catch (RedisException ex)
                {
                    _logger.LogWarning(ex, "Invalidación de Redis pendiente hasta recuperar conexión ({Key})", fullKey);
                }
            }
        }
        finally { _gate.Release(); }
    }

    public void Dispose()
    {
        _redis?.Dispose();
        _gate.Dispose();
    }
}
