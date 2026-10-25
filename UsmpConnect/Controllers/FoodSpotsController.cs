using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

public class FoodSpotsController(
    ApplicationDbContext db,
    CatalogoRestaurantesService catalogo,
    IColaMensajes cola,
    IBuscadorAlgolia algolia,
    IPieHostService pieHost,
    IConfiguration config) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index(CategoriaRestaurante? categoria, string? q)
    {
        var datos = await catalogo.ObtenerAsync();
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

        var vm = new FoodSpotsIndexVM
        {
            Restaurantes = restaurantes.ToList(),
            CategoriaActual = categoria,
            Busqueda = q,
            FuenteDatos = datos.Fuente,
            GoogleMapsApiKey = config["GoogleMaps:ApiKey"] ?? ""
        };

        return View(vm);
    }

    [HttpGet]
    [AllowAnonymous]
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
                x.Usuario.Iniciales,
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
            Categoria = r.Categoria.Nombre(),
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
            EsCampus = (r.Direccion?.Contains("Campus FIA", StringComparison.OrdinalIgnoreCase) ?? false) ||
                       (r.DistanciaTexto?.Contains("Dentro del Campus", StringComparison.OrdinalIgnoreCase) ?? false) ||
                       (r.Nombre?.Contains("Cafetería Central FIA", StringComparison.OrdinalIgnoreCase) ?? false),
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
        await catalogo.InvalidarAsync();

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
