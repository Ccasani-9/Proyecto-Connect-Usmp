using UsmpConnect.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfiguracionServiciosNube
{
    public static IServiceCollection AddServiciosNube(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<EstadoIntegraciones>();
        services.AddSingleton<IRedisCacheService, RedisCacheService>();
        services.AddSingleton<IAnalisisSentimientoService, AnalisisSentimientoService>();
        services.AddSingleton<IRecomendacionRestaurantesService, RecomendacionRestaurantesService>();
        services.AddScoped<CatalogoRestaurantesService>();
        services.AddSingleton<IColaMensajes, CloudAmqpColaMensajes>();
        services.AddHostedService<ConsumidorNotificacionesService>();
        services.AddHttpClient<IBuscadorAlgolia, BuscadorAlgolia>(client => client.Timeout = TimeSpan.FromSeconds(5));
        services.AddSingleton<IPieHostService, PieHostService>();
        services.AddSingleton<UsmpConnect.Services.Conocimiento.DocumentoTextoExtractor>();
        services.AddSingleton<UsmpConnect.Services.Conocimiento.CatalogoConocimiento>();
        services.AddHttpClient("QdrantConocimiento", client => client.Timeout = TimeSpan.FromSeconds(15))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddSingleton<UsmpConnect.Services.Conocimiento.IKnowledgeBaseService,
            UsmpConnect.Services.Conocimiento.KnowledgeBaseService>();

        return services;
    }
}
