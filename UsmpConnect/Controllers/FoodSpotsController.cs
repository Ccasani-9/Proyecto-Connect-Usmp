using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

public class FoodSpotsController(
    ApplicationDbContext db,
    IRedisCacheService cache,
    IColaMensajes cola,
    IBuscadorAlgolia algolia,
    IPieHostService pieHost) : Controller
{
    public async Task<IActionResult> Index(CategoriaRestaurante? categoria, string? q)
    {
        var cacheKey = $"foodspots:list:{categoria?.ToString() ?? "todos"}:{q?.Trim().ToLowerInvariant() ?? "all"}";
        var fuente = "SQLite";

        // 1. Intentar leer desde Redis Cache
        var restaurantesVm = await cache.GetAsync<List<RestauranteItemVM>>(cacheKey);

        if (restaurantesVm != null)
        {
            fuente = "Redis Cache (Hit ⚡)";
        }
        else
        {
            // 2. Consultar SQLite
            var query = db.Restaurantes
                .Include(r => r.Platos)
                .AsNoTracking();

            if (categoria.HasValue)
            {
                query = query.Where(r => r.Categoria == categoria.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(r => EF.Functions.Like(r.Nombre, $"%{term}%") ||
                                         EF.Functions.Like(r.Direccion, $"%{term}%") ||
                                         EF.Functions.Like(r.Descripcion, $"%{term}%"));
            }

            var entidades = await query.ToListAsync();

            restaurantesVm = entidades.Select(r => new RestauranteItemVM(
                r.Id,
                r.Nombre,
                r.Descripcion,
                r.Categoria.ToString(),
                r.Categoria,
                r.Rango,
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
                r.Platos.Select(p => new PlatoItemVM(p.Id, p.Nombre, p.Precio, p.Descripcion, p.EsPopular)).ToList()
            )).ToList();

            // Guardar en Redis Cache por 5 minutos
            await cache.SetAsync(cacheKey, restaurantesVm, TimeSpan.FromMinutes(5));
            fuente = cache.IsConnected ? "SQLite → Guardado en Redis" : "SQLite Local";
        }

        var vm = new FoodSpotsIndexVM
        {
            Restaurantes = restaurantesVm,
            CategoriaActual = categoria,
            Busqueda = q,
            FuenteDatos = fuente
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var r = await db.Restaurantes
            .Include(x => x.Platos)
            .Include(x => x.Resenas)
                .ThenInclude(res => res.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (r == null) return NotFound();

        var resenas = r.Resenas
            .OrderByDescending(x => x.FechaUtc)
            .Select(x => new ResenaItemVM(
                x.Id,
                x.Usuario.Nombres + " " + x.Usuario.Apellidos,
                x.Usuario.Nombres.Substring(0, 1) + x.Usuario.Apellidos.Substring(0, 1),
                x.Puntuacion,
                x.Comentario,
                HoraPeru.Hace(x.FechaUtc)
            )).ToList();

        var platos = r.Platos.Select(p => new PlatoItemVM(p.Id, p.Nombre, p.Precio, p.Descripcion, p.EsPopular)).ToList();

        return Json(new
        {
            r.Id,
            r.Nombre,
            r.Descripcion,
            Categoria = r.Categoria.ToString(),
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
            Platos = platos,
            Resenas = resenas
        });
    }

    [HttpPost]
    public async Task<IActionResult> Resenar(CrearResenaVM vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Verifica los datos de tu reseña.";
            return RedirectToAction(nameof(Index));
        }

        var r = await db.Restaurantes
            .Include(x => x.Resenas)
            .FirstOrDefaultAsync(x => x.Id == vm.RestauranteId);

        if (r == null) return NotFound();

        var userId = User.GetUserId();

        var nuevaResena = new ResenaRestaurante
        {
            RestauranteId = vm.RestauranteId,
            UsuarioId = userId,
            Puntuacion = vm.Puntuacion,
            Comentario = vm.Comentario.Trim(),
            FechaUtc = DateTime.UtcNow
        };

        db.ResenasRestaurantes.Add(nuevaResena);
        await db.SaveChangesAsync();

        // Recalcular promedio
        var todas = await db.ResenasRestaurantes.Where(x => x.RestauranteId == vm.RestauranteId).ToListAsync();
        r.TotalResenas = todas.Count;
        r.CalificacionPromedio = Math.Round(todas.Average(x => x.Puntuacion), 1);
        await db.SaveChangesAsync();

        // 1. Invalidar caché en Redis
        await cache.RemoveAsync("foodspots:list:todos:all");

        // 2. Encolar evento asíncrono en CloudAMQP (RabbitMQ)
        await cola.PublicarAsync("usmp_notificaciones_cola", new
        {
            Evento = "NuevaResenaRestaurante",
            Restaurante = r.Nombre,
            UsuarioId = userId,
            Puntuacion = vm.Puntuacion,
            Fecha = DateTime.UtcNow
        });

        // 3. Emitir actualización en vivo por WebSockets (PieHost)
        await pieHost.PublicarEventoAsync("1", "nueva_resena", new
        {
            RestauranteId = r.Id,
            RestauranteNombre = r.Nombre,
            NuevaCalificacion = r.CalificacionPromedio,
            TotalResenas = r.TotalResenas,
            Usuario = User.GetNombre()
        });

        TempData["Toast"] = $"¡Reseña registrada para {r.Nombre}! Gracias por tu aporte a la comunidad.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Buscar(string q)
    {
        var resultados = await algolia.BuscarAsync(q, db);
        return Json(resultados);
    }

    [HttpPost]
    public async Task<IActionResult> SincronizarAlgolia()
    {
        await algolia.SincronizarTodoAsync(db);
        TempData["Toast"] = "Catálogo sincronizado exitosamente con Algolia Search Engine.";
        return RedirectToAction(nameof(Index));
    }
}
