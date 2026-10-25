using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

public class FoodSpotsController(
    ApplicationDbContext db,
    CatalogoRestaurantesService catalogo,
    IBuscadorAlgolia algolia,
    IConfiguration config) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index(CategoriaRestaurante? categoria, string? q)
    {
        var (restaurantes, fuente) = await catalogo.FiltrarAsync(categoria, q);
        var usuarioClave = !string.IsNullOrWhiteSpace(User?.Identity?.Name)
            ? User.Identity!.Name
            : User?.GetUserId();
        var (recomendados, personalizada) = await catalogo.ObtenerRecomendadosAsync(usuarioClave, topK: 3);

        var vm = new FoodSpotsIndexVM
        {
            Restaurantes = restaurantes,
            Recomendados = recomendados,
            RecomendacionPersonalizada = personalizada,
            CategoriaActual = categoria,
            Busqueda = q,
            FuenteDatos = fuente,
            GoogleMapsApiKey = config["GoogleMaps:ApiKey"] ?? ""
        };

        return View(vm);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Detalle(int id)
    {
        var detalle = await catalogo.ObtenerDetalleAsync(id);
        return detalle is null ? NotFound() : Json(detalle);
    }

    [HttpPost]
    public async Task<IActionResult> Resenar(CrearResenaVM vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Verifica los datos de tu reseña.";
            return RedirectToAction(nameof(Index));
        }

        var creada = await catalogo.CrearResenaAsync(
            vm.RestauranteId,
            User.GetUserId(),
            User.GetNombre(),
            vm.Puntuacion,
            vm.Comentario);

        if (creada is null)
            return NotFound();

        TempData["Toast"] = $"¡Reseña registrada para {creada.RestauranteNombre}! Gracias por tu aporte a la comunidad.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Buscar(string q)
    {
        var resultados = await algolia.BuscarAsync(q, db);
        return Json(resultados);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Profesor)]
    public async Task<IActionResult> SincronizarAlgolia()
    {
        var aceptada = await algolia.SincronizarTodoAsync(db);
        TempData[aceptada ? "Toast" : "ToastError"] = aceptada ? "Sincronización del catálogo solicitada." : "No se pudo sincronizar el catálogo. Revisa la configuración del servicio.";
        return RedirectToAction(nameof(Index));
    }
}
