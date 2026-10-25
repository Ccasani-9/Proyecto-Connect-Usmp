using UsmpConnect.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfiguracionServiciosNube
{
    public static IServiceCollection AddServiciosNube(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IRedisCacheService, RedisCacheService>();
        services.AddScoped<CatalogoRestaurantesService>();
        services.AddSingleton<IColaMensajes, CloudAmqpColaMensajes>();
        services.AddHttpClient<IBuscadorAlgolia, BuscadorAlgolia>(client => client.Timeout = TimeSpan.FromSeconds(5));
        services.AddSingleton<IPieHostService, PieHostService>();

        return services;
    }
}
