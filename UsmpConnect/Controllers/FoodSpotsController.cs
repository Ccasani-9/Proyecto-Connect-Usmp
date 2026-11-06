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
    IConfiguration config,
    RutasAPieService rutas) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index(CategoriaRestaurante? categoria, string? q,
        double? valoracionMinima = null, decimal? gastoMaximo = null, int? minutosMaximos = null)
    {
        valoracionMinima = valoracionMinima is 3.5 or 4 or 4.5 ? valoracionMinima : null;
        gastoMaximo = gastoMaximo is > 0 ? gastoMaximo : null;
        var ct = HttpContext.RequestAborted;
        var (restaurantes, fuente) = await catalogo.FiltrarAsync(categoria, q, ct,
            valoracionMinima: valoracionMinima, gastoMaximo: gastoMaximo);
        var rutasDisponibles = rutas.Disponible && User.IsInRole(Roles.Alumno);
        minutosMaximos = rutasDisponibles && minutosMaximos is >= 1 and <= 60 ? minutosMaximos : null;
        ResultadoRutasAPie? resultadoRutas = null;
        var totalCandidatos = restaurantes.Count;
        if (minutosMaximos.HasValue && totalCandidatos > 0)
        {
            Response.Headers.CacheControl = "no-store";
            resultadoRutas = await rutas.ConsultarAsync(restaurantes, ct);
            if (resultadoRutas.MinutosPorLocal.Count > 0)
                restaurantes = restaurantes.Where(r => resultadoRutas.MinutosPorLocal.TryGetValue(r.Id, out var minutos)
                    && minutos <= minutosMaximos.Value).ToList();
        }
        var usuarioClave = !string.IsNullOrWhiteSpace(User?.Identity?.Name)
            ? User.Identity!.Name
            : User?.GetUserId();
        var rutasAplicadas = resultadoRutas is { MinutosPorLocal.Count: > 0 };
        var (recomendados, personalizada) = await catalogo.ObtenerRecomendadosAsync(usuarioClave,
            cancellationToken: ct, candidatos: rutasAplicadas ? restaurantes : null);

        var vm = new FoodSpotsIndexVM
        {
            Restaurantes = restaurantes,
            Recomendados = recomendados,
            RecomendacionPersonalizada = personalizada,
            CategoriaActual = categoria,
            Busqueda = q,
            ValoracionMinima = valoracionMinima,
            GastoMaximo = gastoMaximo,
            MinutosMaximos = minutosMaximos,
            RutasDisponibles = rutasDisponibles,
            ResultadoRutas = resultadoRutas,
            TotalCandidatosRuta = totalCandidatos,
            FuenteDatos = fuente,
            GoogleMapsApiKey = config["GoogleMaps:ApiKey"] ?? ""
        };

        return View(vm);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult CondicionesRutas() => View();

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

}
