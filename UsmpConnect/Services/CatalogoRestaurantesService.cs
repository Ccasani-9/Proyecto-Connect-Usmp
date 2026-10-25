using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Services;

public record CatalogoRestaurantes(List<RestauranteItemVM> Restaurantes, string Fuente);

/// <summary>Una copia del catálogo para todos los filtros; una reseña invalida la copia completa.</summary>
public sealed class CatalogoRestaurantesService(
    ApplicationDbContext db,
    IRedisCacheService cache,
    IBuscadorAlgolia algolia,
    IColaMensajes cola,
    ILogger<CatalogoRestaurantesService> logger)
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

    public async Task<(List<RestauranteItemVM> Restaurantes, string Fuente)> FiltrarAsync(
        CategoriaRestaurante? categoria, string? q, CancellationToken cancellationToken = default)
    {
        var datos = await ObtenerAsync(cancellationToken);
        IEnumerable<RestauranteItemVM> restaurantes = datos.Restaurantes;
        if (categoria.HasValue)
            restaurantes = restaurantes.Where(r => r.Categoria == categoria.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            restaurantes = restaurantes.Where(r => r.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase)
                || r.Direccion.Contains(term, StringComparison.OrdinalIgnoreCase)
                || r.Descripcion.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return (restaurantes.ToList(), datos.Fuente);
    }

    public async Task<RestauranteDetalleDto?> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default)
    {
        var r = await db.Restaurantes
            .Include(x => x.Platos)
            .Include(x => x.Resenas)
                .ThenInclude(res => res.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (r is null)
            return null;

        var resenas = r.Resenas
            .OrderByDescending(x => x.FechaUtc)
            .Select(x => new ResenaItemVM(
                x.Id,
                $"{x.Usuario.Nombres} {x.Usuario.Apellidos}".Trim(),
                x.Usuario.Iniciales,
                x.Puntuacion,
                x.Comentario,
                HoraPeru.Hace(x.FechaUtc)
            )).ToList();

        var platos = r.Platos
            .Select(p => new PlatoItemVM(p.Id, p.Nombre, p.Precio, p.Descripcion, p.EsPopular))
            .ToList();

        var esCampus = r.Direccion.Contains("Campus FIA", StringComparison.OrdinalIgnoreCase) ||
                       r.DistanciaTexto.Contains("Dentro del Campus", StringComparison.OrdinalIgnoreCase) ||
                       r.Nombre.Contains("Cafetería Central FIA", StringComparison.OrdinalIgnoreCase);

        return new RestauranteDetalleDto(
            r.Id,
            r.Nombre,
            r.Descripcion,
            r.Categoria.Nombre(),
            r.Direccion,
            r.DistanciaTexto,
            r.RangoPrecios,
            r.HorarioAtencion,
            r.TelefonoWhatsApp,
            r.FotoUrl,
            r.Latitud,
            r.Longitud,
            r.CalificacionPromedio,
            r.TotalResenas,
            esCampus,
            platos,
            resenas);
    }

    public async Task<ResenaDto?> CrearResenaAsync(
        int restauranteId,
        string usuarioId,
        string usuarioNombre,
        int puntuacion,
        string comentario,
        CancellationToken cancellationToken = default)
    {
        var r = await db.Restaurantes.FirstOrDefaultAsync(x => x.Id == restauranteId, cancellationToken);
        if (r is null)
            return null;

        var nuevaResena = new ResenaRestaurante
        {
            RestauranteId = restauranteId,
            UsuarioId = usuarioId,
            Puntuacion = puntuacion,
            Comentario = comentario.Trim(),
            FechaUtc = DateTime.UtcNow
        };

        db.ResenasRestaurantes.Add(nuevaResena);
        await db.SaveChangesAsync(cancellationToken);

        await RecalcularCalificacionRestauranteAsync(r, cancellationToken);
        await SincronizarCambiosAsync();

        await cola.PublicarAsync(
            CloudAmqpColaMensajes.ColaPorDefecto,
            new EventoResenaCola(
                nuevaResena.Id,
                r.Id,
                r.Nombre,
                usuarioId,
                usuarioNombre,
                puntuacion,
                r.CalificacionPromedio,
                r.TotalResenas,
                nuevaResena.FechaUtc));

        return new ResenaDto(
            nuevaResena.Id,
            r.Id,
            r.Nombre,
            usuarioId,
            usuarioNombre,
            nuevaResena.Puntuacion,
            nuevaResena.Comentario,
            nuevaResena.FechaUtc,
            r.CalificacionPromedio,
            r.TotalResenas);
    }

    public async Task<(EstadoOperacionResena Estado, ResenaDto? Resena)> EditarResenaPropiaAsync(
        int resenaId,
        string usuarioId,
        string usuarioNombre,
        int puntuacion,
        string comentario,
        CancellationToken cancellationToken = default)
    {
        var resena = await db.ResenasRestaurantes
            .Include(x => x.Restaurante)
            .FirstOrDefaultAsync(x => x.Id == resenaId, cancellationToken);

        if (resena is null)
            return (EstadoOperacionResena.NoEncontrada, null);

        if (!string.Equals(resena.UsuarioId, usuarioId, StringComparison.Ordinal))
            return (EstadoOperacionResena.Prohibida, null);

        resena.Puntuacion = puntuacion;
        resena.Comentario = comentario.Trim();
        resena.FechaUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await RecalcularCalificacionRestauranteAsync(resena.Restaurante, cancellationToken);
        await SincronizarCambiosAsync();

        var dto = new ResenaDto(
            resena.Id,
            resena.RestauranteId,
            resena.Restaurante.Nombre,
            resena.UsuarioId,
            usuarioNombre,
            resena.Puntuacion,
            resena.Comentario,
            resena.FechaUtc,
            resena.Restaurante.CalificacionPromedio,
            resena.Restaurante.TotalResenas);

        return (EstadoOperacionResena.Ok, dto);
    }

    public async Task<EstadoOperacionResena> EliminarResenaPropiaAsync(
        int resenaId,
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        var resena = await db.ResenasRestaurantes
            .Include(x => x.Restaurante)
            .FirstOrDefaultAsync(x => x.Id == resenaId, cancellationToken);

        if (resena is null)
            return EstadoOperacionResena.NoEncontrada;

        if (!string.Equals(resena.UsuarioId, usuarioId, StringComparison.Ordinal))
            return EstadoOperacionResena.Prohibida;

        var restaurante = resena.Restaurante;
        db.ResenasRestaurantes.Remove(resena);
        await db.SaveChangesAsync(cancellationToken);

        await RecalcularCalificacionRestauranteAsync(restaurante, cancellationToken);
        await SincronizarCambiosAsync();

        return EstadoOperacionResena.Ok;
    }

    private async Task RecalcularCalificacionRestauranteAsync(Restaurante restaurante, CancellationToken cancellationToken)
    {
        var puntuaciones = await db.ResenasRestaurantes
            .Where(x => x.RestauranteId == restaurante.Id)
            .Select(x => x.Puntuacion)
            .ToListAsync(cancellationToken);

        restaurante.TotalResenas = puntuaciones.Count;
        restaurante.CalificacionPromedio = puntuaciones.Count == 0
            ? 0
            : Math.Round(puntuaciones.Average(), 1);

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SincronizarCambiosAsync()
    {
        await InvalidarAsync();
        await algolia.SincronizarTodoAsync(db);
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
