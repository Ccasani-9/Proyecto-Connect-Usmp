using UsmpConnect.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfiguracionServiciosNube
{
    public static IServiceCollection AddServiciosNube(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IRedisCacheService, RedisCacheService>();
        services.AddSingleton<IColaMensajes, CloudAmqpColaMensajes>();
        services.AddSingleton<IBuscadorAlgolia, BuscadorAlgolia>();
        services.AddSingleton<IPieHostService, PieHostService>();

        return services;
    }
}
