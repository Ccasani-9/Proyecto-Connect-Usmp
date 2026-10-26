using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

public class HomeController(
    ApplicationDbContext db,
    CatalogoRestaurantesService catalogoRestaurantes) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var usuarioClave = !string.IsNullOrWhiteSpace(User.Identity?.Name)
            ? User.Identity!.Name
            : userId;
        var hoy = HoraPeru.Hoy;
        var hora = HoraPeru.Ahora.Hour;

        var semana = await Agenda.ClasesSemanaAsync(db, userId, false);
        var eventos = await Agenda.EventosAsync(db, userId);

        var nombre = User.GetNombre();
        var primerNombre = !string.IsNullOrWhiteSpace(nombre) ? (nombre.Split(' ').FirstOrDefault() ?? "") : "Comunidad USMP";

        var (recomendados, personalizada) = await catalogoRestaurantes.ObtenerRecomendadosAsync(usuarioClave, topK: 3);

        var vm = new InicioViewModel
        {
            PrimerNombre = primerNombre,
            Saludo = hora < 12 ? "Buenos días" : hora < 19 ? "Buenas tardes" : "Buenas noches",
            EsProfesor = false,
            Hoy = Agenda.Dia(semana, hoy, "HOY"),
            Manana = Agenda.Dia(semana, hoy.AddDays(1), "MAÑANA"),
            Eventos = eventos,
            ProximosEventos = eventos.Where(e => DateOnly.Parse(e.Fecha) >= hoy).Take(4).ToList(),
            FechaHoy = hoy,
            HuariqueRecomendado = recomendados.FirstOrDefault(),
            AlternativasRecomendadas = recomendados.Skip(1).ToList(),
            RecomendacionPersonalizada = personalizada
        };

        return View(vm);
    }

    /// <summary>Páginas de estado (404 → "módulo en construcción").</summary>
    [AllowAnonymous]
    public IActionResult Estado(int id)
    {
        Response.StatusCode = id;
        ViewData["Codigo"] = id;
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
