using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Services;

public record CatalogoRestaurantes(List<RestauranteItemVM> Restaurantes, string Fuente);

/// <summary>Una copia del catálogo para todos los filtros; una reseña invalida la copia completa.</summary>
public sealed class CatalogoRestaurantesService(
    ApplicationDbContext db, IRedisCacheService cache, ILogger<CatalogoRestaurantesService> logger)
{
    public const string CacheKey = "foodspots:catalogo:v1";
    // Evita que una carga en curso vuelva a guardar una copia vieja después de invalidar
    // dentro de este proceso. Redis sigue compartiendo la clave entre instancias.
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public async Task<CatalogoRestaurantes> ObtenerAsync(CancellationToken cancellationToken = default)
    {
        await Gate.WaitAsync(cancellationToken);
        try
        {
            var items = await cache.GetAsync<List<RestauranteItemVM>>(CacheKey);
            if (items is not null)
            {
                var fuente = cache.IsConnected ? "Redis Cache" : "Caché local";
                logger.LogDebug("Catálogo de restaurantes: hit en {Fuente}", fuente);
                return new(items, fuente);
            }

            var entidades = await db.Restaurantes.Include(r => r.Platos).AsNoTracking()
                .OrderBy(r => r.Id).ToListAsync(cancellationToken);
            items = entidades.Select(r => new RestauranteItemVM(
                r.Id, r.Nombre, r.Descripcion, r.Categoria.ToString(), r.Categoria, r.Rango,
                r.Direccion, r.DistanciaTexto, r.RangoPrecios, r.HorarioAtencion, r.TelefonoWhatsApp,
                r.FotoUrl, r.Latitud, r.Longitud, r.CalificacionPromedio, r.TotalResenas,
                r.Platos.Select(p => new PlatoItemVM(p.Id, p.Nombre, p.Precio, p.Descripcion, p.EsPopular)).ToList()
            )).ToList();
            await cache.SetAsync(CacheKey, items, TimeSpan.FromMinutes(5));
            logger.LogInformation("Catálogo cargado desde SQLite: {Cantidad} restaurantes; {Estado}", items.Count, cache.Estado);
            return new(items, "SQLite");
        }
        finally { Gate.Release(); }
    }

    public async Task InvalidarAsync()
    {
        await Gate.WaitAsync();
        try
        {
            await cache.RemoveAsync(CacheKey);
            logger.LogInformation("Catálogo de restaurantes invalidado para todas las categorías y búsquedas");
        }
        finally { Gate.Release(); }
    }

    public async Task PrecalentarCacheAsync(CancellationToken cancellationToken = default)
    {
        var catalogo = await ObtenerAsync(cancellationToken);
        logger.LogInformation("Precarga de restaurantes completada: {Cantidad} elementos; {Estado}",
            catalogo.Restaurantes.Count, cache.Estado);
    }
}
