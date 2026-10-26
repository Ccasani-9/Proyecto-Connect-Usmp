using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using UsmpConnect.Services;

namespace UsmpConnect.Tests;

public class RedisCacheTests
{
    [RedisFact]
    [Trait("Category", "Redis")]
    public async Task InvalidacionSinConexion_EliminaLaCopiaRemotaAlReconectar()
    {
        var options = ConfigurationOptions.Parse(Environment.GetEnvironmentVariable("USMP_TEST_REDIS")!);
        var endpoint = options.EndPoints.Single();
        var host = endpoint is System.Net.DnsEndPoint dns ? dns.Host : ((System.Net.IPEndPoint)endpoint).Address.ToString();
        var port = endpoint is System.Net.DnsEndPoint dns2 ? dns2.Port : ((System.Net.IPEndPoint)endpoint).Port;
        await using var proxy = new RedisTestProxy(host, port);
        using var memory = new MemoryCache(new MemoryCacheOptions());
        using var cache = Create(memory, $"{proxy.Endpoint},abortConnect=false");
        var key = $"tests:{Guid.NewGuid():N}";
        try
        {
            await cache.SetAsync(key, "copia anterior");
            proxy.Disconnect();
            await WaitAsync(() => !cache.IsConnected);
            await cache.RemoveAsync(key);
            Assert.Null(await cache.GetAsync<string>(key));
            proxy.Reconnect();
            await WaitAsync(() => cache.IsConnected);
            Assert.Null(await cache.GetAsync<string>(key));
        }
        finally { proxy.Reconnect(); await cache.RemoveAsync(key); }
    }

    private static async Task WaitAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(20);
        while (!condition() && DateTime.UtcNow < deadline) await Task.Delay(50);
        Assert.True(condition(), "La conexión Redis no alcanzó el estado esperado.");
    }

    [Fact]
    public async Task SinRedis_UsaMemoriaYPermiteInvalidar()
    {
        using var memory = new MemoryCache(new MemoryCacheOptions());
        var cache = Create(memory, "");
        Assert.False(cache.IsConnected);
        await cache.SetAsync("prueba", new[] { "restaurante" });
        Assert.Equal(new[] { "restaurante" }, await cache.GetAsync<string[]>("prueba"));
        await cache.RemoveAsync("prueba");
        Assert.Null(await cache.GetAsync<string[]>("prueba"));
        (cache as IDisposable)?.Dispose();
    }

    [RedisFact]
    [Trait("Category", "Redis")]
    public async Task EliminacionEnOtraInstancia_NoReviveSnapshotLocal()
    {
        using var memoryA = new MemoryCache(new MemoryCacheOptions());
        using var memoryB = new MemoryCache(new MemoryCacheOptions());
        var a = Create(memoryA);
        var b = Create(memoryB);
        var key = $"tests:{Guid.NewGuid():N}";
        try
        {
            Assert.True(a.IsConnected);
            await a.SetAsync(key, "dato antiguo");
            await b.RemoveAsync(key);
            Assert.Null(await a.GetAsync<string>(key));
        }
        finally { await a.RemoveAsync(key); (a as IDisposable)?.Dispose(); (b as IDisposable)?.Dispose(); }
    }

    [RedisFact]
    [Trait("Category", "Redis")]
    public async Task JsonCorrupto_NoDevuelveSnapshotAntiguo()
    {
        using var memory = new MemoryCache(new MemoryCacheOptions());
        var cache = Create(memory);
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("USMP_TEST_REDIS")!);
        var key = $"tests:{Guid.NewGuid():N}";
        try
        {
            await cache.SetAsync(key, "dato antiguo");
            await redis.GetDatabase().StringSetAsync($"usmp:{key}", "no-es-json");
            Assert.Null(await cache.GetAsync<string>(key));
        }
        finally { await cache.RemoveAsync(key); (cache as IDisposable)?.Dispose(); }
    }

    [RedisFact]
    [Trait("Category", "Redis")]
    public async Task ExpiracionRemota_NoReutilizaFallbackConVidaMayor()
    {
        using var memory = new MemoryCache(new MemoryCacheOptions());
        var cache = Create(memory);
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("USMP_TEST_REDIS")!);
        var key = $"tests:{Guid.NewGuid():N}";
        try
        {
            await cache.SetAsync(key, "dato", TimeSpan.FromMinutes(1));
            await redis.GetDatabase().KeyExpireAsync($"usmp:{key}", TimeSpan.FromMilliseconds(100));
            await Task.Delay(400);
            Assert.Null(await cache.GetAsync<string>(key));
        }
        finally { await cache.RemoveAsync(key); (cache as IDisposable)?.Dispose(); }
    }

    private static RedisCacheService Create(IMemoryCache memory, string? connection = null) =>
        new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Redis:ConnectionString"] = connection ?? Environment.GetEnvironmentVariable("USMP_TEST_REDIS")
        }).Build(), memory, NullLogger<RedisCacheService>.Instance);
}

public sealed class RedisFactAttribute : FactAttribute
{
    public RedisFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("USMP_TEST_REDIS")))
            Skip = "Configura USMP_TEST_REDIS con un Redis desechable para las pruebas de integración.";
    }
}
