using System.Text.RegularExpressions;
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
    IPieHostService pieHost,
    IAnalisisSentimientoService sentimiento,
    IRecomendacionRestaurantesService recomendador,
    ILogger<CatalogoRestaurantesService> logger)
{
    public const string CacheKey = "foodspots:catalogo:v2";
    // Evita que una carga en curso vuelva a guardar una copia vieja después de invalidar
    // dentro de este proceso. Redis sigue compartiendo la clave entre instancias.
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly Regex[] FamiliasDeComida =
    [
        Familia("pizzas?|pizzer[ií]as?|pastas?|lasa(?:ñ|gn|gñ)as?|trattorias?|ravioles?|fetuccinis?"),
        Familia("ceviches?|cebiches?|mariscos?|chicharr[oó]n de pescado|tr[ií]os? marinos?"),
        Familia("sushis?|makis?|ramen|nikkei|sashimi|nigiris?"),
        Familia("chifas?|chaufa|aeropuerto|want[aá]n|tallar[ií]n saltado"),
        Familia("pollos? a la brasa|broaster|poller[ií]as?"),
        Familia("hamburguesas?|salchipapas?|sandwich(?:es)?|s[aá]nguches?"),
        Familia("caf[eé]s?|cafeter[ií]as?|espresso|lattes?|capuchinos?"),
        Familia("pasteles?|pasteler[ií]as?|tortas?|postres?|helados?|brownies?"),
        Familia("men[uú]s? criollos?|men[uú]s? caseros?|lomo saltado|aj[ií] de gallina|seco de res"),
        Familia("bowls?|ensaladas?|quinua|quinoa|vegetarian[oa]s?")
    ];

    private static Regex Familia(string platos) => new($@"\b(?:{platos})\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static string TextoDeComida(RestauranteItemVM local) => string.Join(" ",
        new[] { local.Nombre, local.Descripcion }.Concat(local.PlatosPopulares.Select(p => $"{p.Nombre} {p.Descripcion}")));

    public async Task<CatalogoRestaurantes> ObtenerAsync(CancellationToken cancellationToken = default)
    {
        await Gate.WaitAsync(cancellationToken);
        try
        {
            var lectura = await cache.LeerConOrigenAsync<List<RestauranteItemVM>>(CacheKey);
            var items = lectura.Valor;
            if (items is not null)
            {
                var fuente = lectura.Origen;
                logger.LogDebug("Catálogo de restaurantes: hit en {Fuente}", fuente);
                return new(items, fuente);
            }

            var entidades = await db.Restaurantes.Include(r => r.Platos).AsNoTracking()
                .OrderBy(r => r.Id).ToListAsync(cancellationToken);
            items = entidades.Select(r => new RestauranteItemVM(
                r.Id, r.Nombre, r.Descripcion, r.Categoria.Nombre(), r.Categoria, r.Rango,
                r.Direccion, r.DistanciaTexto, r.RangoPrecios, r.HorarioAtencion, r.TelefonoWhatsApp,
                r.FotoUrl, r.Latitud, r.Longitud, r.CalificacionPromedio, r.TotalResenas,
                r.Platos.Select(p => new PlatoItemVM(p.Id, p.Nombre, p.Precio, p.Descripcion, p.EsPopular)).ToList(),
                r.PrecioPromedioReferencial
            )).ToList();
            await cache.SetAsync(CacheKey, items, TimeSpan.FromMinutes(5));
            logger.LogInformation("Catálogo cargado desde SQLite: {Cantidad} restaurantes; {Estado}", items.Count, cache.Estado);
            return new(items, "SQLite");
        }
        finally { Gate.Release(); }
    }

    public async Task<(List<RestauranteItemVM> Recomendados, bool Personalizada)> ObtenerRecomendadosAsync(
        string? usuarioClave,
        int topK = 3,
        CancellationToken cancellationToken = default,
        IReadOnlyList<RestauranteItemVM>? candidatos = null,
        string? usuarioId = null)
    {
        var locales = candidatos ?? (await ObtenerAsync(cancellationToken)).Restaurantes;
        Dictionary<Regex, double> preferencias = [];
        if (!string.IsNullOrWhiteSpace(usuarioId))
        {
            var resenas = await db.ResenasRestaurantes.AsNoTracking().Where(r => r.UsuarioId == usuarioId)
                .Select(r => new { r.RestauranteId, r.Puntuacion }).ToListAsync(cancellationToken);
            if (resenas.Count > 0)
            {
                var historial = resenas.GroupBy(r => r.RestauranteId)
                    .ToDictionary(g => g.Key, g => g.Average(r => r.Puntuacion));
                var catalogoCompleto = candidatos is null ? locales : (await ObtenerAsync(cancellationToken)).Restaurantes;
                var favoritos = catalogoCompleto.Where(r => historial.GetValueOrDefault(r.Id) >= 4);
                foreach (var favorito in favoritos)
                {
                    var texto = TextoDeComida(favorito);
                    foreach (var familia in FamiliasDeComida.Where(f => f.IsMatch(texto)))
                        preferencias[familia] = preferencias.GetValueOrDefault(familia) + historial[favorito.Id] - 3;
                }

                locales = locales.Where(r => !historial.ContainsKey(r.Id)).ToList();
            }
        }
        if (locales.Count == 0)
            return ([], false);

        var porId = locales.ToDictionary(r => r.Id);
        var recs = await recomendador.RecomendarAsync(
            usuarioClave,
            porId.Keys,
            preferencias.Count > 0 ? locales.Count : topK,
            cancellationToken);

        if (preferencias.Count > 0)
        {
            var afinidades = locales.ToDictionary(r => r.Id, r =>
            {
                var texto = TextoDeComida(r);
                return preferencias.Where(p => p.Key.IsMatch(texto)).Sum(p => p.Value);
            });
            if (afinidades.Values.Any(a => a > 0))
            {
                var posiciones = recs.Select((r, i) => (r.RestauranteId, i))
                    .ToDictionary(r => r.RestauranteId, r => r.i);
                var sugerencias = locales.OrderByDescending(r => afinidades[r.Id])
                    .ThenBy(r => posiciones.GetValueOrDefault(r.Id, int.MaxValue))
                    .ThenByDescending(r => r.CalificacionPromedio)
                    .ThenBy(r => r.Id).Take(Math.Max(1, topK)).ToList();
                return (sugerencias, true);
            }
        }

        if (recs.Count > 0)
        {
            var ordenados = recs
                .Where(r => porId.ContainsKey(r.RestauranteId))
                .Select(r => porId[r.RestauranteId])
                .Take(Math.Max(1, topK))
                .ToList();

            var personalizada = recs.Any(r => !r.DeFrio);
            return (ordenados, personalizada);
        }

        // Respaldo por calificación promedio si el recomendador está deshabilitado
        var populares = locales
            .OrderByDescending(r => r.CalificacionPromedio)
            .ThenByDescending(r => r.TotalResenas)
            .Take(Math.Max(1, topK))
            .ToList();

        return (populares, false);
    }

    public async Task<(List<RestauranteItemVM> Restaurantes, string Fuente)> FiltrarAsync(
        CategoriaRestaurante? categoria, string? q, CancellationToken cancellationToken = default,
        double? valoracionMinima = null, decimal? gastoMaximo = null)
    {
        var datos = await ObtenerAsync(cancellationToken);
        IEnumerable<RestauranteItemVM> restaurantes = datos.Restaurantes;
        if (categoria.HasValue)
            restaurantes = restaurantes.Where(r => r.Categoria == categoria.Value);
        if (valoracionMinima.HasValue)
            restaurantes = restaurantes.Where(r => r.TotalResenas > 0 && r.CalificacionPromedio >= valoracionMinima.Value);
        if (gastoMaximo.HasValue)
            restaurantes = restaurantes.Where(r => r.PrecioPromedioReferencial is > 0 && r.PrecioPromedioReferencial <= gastoMaximo.Value);
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
                HoraPeru.Hace(x.FechaUtc),
                x.Sentimiento,
                x.ProbabilidadSentimiento,
                x.UsuarioId
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
            resenas,
            r.PrecioPromedioReferencial);
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

        var analisis = await sentimiento.AnalizarAsync(comentario, cancellationToken);

        var nuevaResena = new ResenaRestaurante
        {
            RestauranteId = restauranteId,
            UsuarioId = usuarioId,
            Puntuacion = puntuacion,
            Comentario = comentario.Trim(),
            Sentimiento = analisis.Etiqueta,
            ProbabilidadSentimiento = analisis.Analizado ? analisis.Probabilidad : null,
            FechaUtc = DateTime.UtcNow
        };

        db.ResenasRestaurantes.Add(nuevaResena);
        await db.SaveChangesAsync(cancellationToken);

        await RecalcularCalificacionRestauranteAsync(r, cancellationToken);
        await SincronizarCambiosAsync();

        await cola.PublicarAsync(
            string.Empty, // La cola configurada se resuelve en el productor.
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
            r.TotalResenas,
            nuevaResena.Sentimiento,
            nuevaResena.ProbabilidadSentimiento);
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

        var analisis = await sentimiento.AnalizarAsync(comentario, cancellationToken);

        resena.Puntuacion = puntuacion;
        resena.Comentario = comentario.Trim();
        resena.Sentimiento = analisis.Etiqueta;
        resena.ProbabilidadSentimiento = analisis.Analizado ? analisis.Probabilidad : null;
        resena.FechaUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await RecalcularCalificacionRestauranteAsync(resena.Restaurante, cancellationToken);
        await SincronizarCambiosAsync();
        await AvisarActualizacionResenasAsync(resena.RestauranteId);

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
            resena.Restaurante.TotalResenas,
            resena.Sentimiento,
            resena.ProbabilidadSentimiento);

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
        await AvisarActualizacionResenasAsync(restaurante.Id);

        return EstadoOperacionResena.Ok;
    }

    private Task AvisarActualizacionResenasAsync(int restauranteId) => pieHost.IsConfigured
        ? pieHost.PublicarEventoAsync("1", "resenas_actualizadas", new { RestauranteId = restauranteId })
        : Task.CompletedTask;

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
