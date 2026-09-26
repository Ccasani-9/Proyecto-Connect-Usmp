using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace UsmpConnect.Services;

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T data, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    bool IsConnected { get; }
    string Estado { get; }
}

public class RedisCacheService : IRedisCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IConnectionMultiplexer? _redis;
    private readonly IDatabase? _db;
    private readonly bool _usandoRedis;

    public bool IsConnected => _usandoRedis && (_redis?.IsConnected ?? false);
    public string Estado => IsConnected ? "Redis Cloud (Conectado)" : "Caché en Memoria (Fallback)";

    public RedisCacheService(IConfiguration config, IMemoryCache memoryCache, ILogger<RedisCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;

        var connStr = config["Redis:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            try
            {
                var options = ConfigurationOptions.Parse(connStr);
                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 4000;
                options.SyncTimeout = 4000;
                _redis = ConnectionMultiplexer.Connect(options);
                _db = _redis.GetDatabase();
                _usandoRedis = true;
                _logger.LogInformation("Redis conectado exitosamente a {Endpoint}", options.EndPoints.FirstOrDefault());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo conectar a Redis Cloud. Usando MemoryCache como fallback.");
                _usandoRedis = false;
            }
        }
        else
        {
            _usandoRedis = false;
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var fullKey = $"usmp:{key}";
        if (IsConnected && _db != null)
        {
            try
            {
                var val = await _db.StringGetAsync(fullKey);
                if (val.HasValue)
                {
                    return JsonSerializer.Deserialize<T>((string)val!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al leer de Redis ({Key}). Leyendo de memoria.", fullKey);
            }
        }

        // Fallback en memoria
        return _memoryCache.TryGetValue(fullKey, out T? localVal) ? localVal : default;
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan? expiry = null)
    {
        var fullKey = $"usmp:{key}";
        var json = JsonSerializer.Serialize(data);
        var tiempo = expiry ?? TimeSpan.FromMinutes(10);

        if (IsConnected && _db != null)
        {
            try
            {
                await _db.StringSetAsync(fullKey, json, tiempo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al escribir en Redis ({Key}). Guardando en memoria.", fullKey);
            }
        }

        _memoryCache.Set(fullKey, data, tiempo);
    }

    public async Task RemoveAsync(string key)
    {
        var fullKey = $"usmp:{key}";
        if (IsConnected && _db != null)
        {
            try
            {
                await _db.KeyDeleteAsync(fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al borrar en Redis ({Key}).", fullKey);
            }
        }

        _memoryCache.Remove(fullKey);
    }
}
