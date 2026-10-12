using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

public class HomeController(ApplicationDbContext db) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var hoy = HoraPeru.Hoy;
        var hora = HoraPeru.Ahora.Hour;

        var semana = await Agenda.ClasesSemanaAsync(db, userId, false);
        var eventos = await Agenda.EventosAsync(db, userId);

        var nombre = User.GetNombre();
        var primerNombre = !string.IsNullOrWhiteSpace(nombre) ? (nombre.Split(' ').FirstOrDefault() ?? "") : "Comunidad USMP";

        var huariqueEntidad = await db.Restaurantes
            .Include(r => r.Platos)
            .AsNoTracking()
            .OrderByDescending(r => r.CalificacionPromedio)
            .FirstOrDefaultAsync();

        RestauranteItemVM? huariqueRecomendado = null;
        if (huariqueEntidad != null)
        {
            var catTexto = huariqueEntidad.Categoria switch
            {
                CategoriaRestaurante.MenuCriollo => "Menú Criollo",
                CategoriaRestaurante.ComidaRapida => "Comida Rápida",
                CategoriaRestaurante.Chifa => "Chifa",
                CategoriaRestaurante.CafeteriaHuarique => "Cafetería & Huarique",
                CategoriaRestaurante.PasteleriaPostres => "Pastelería & Postres",
                CategoriaRestaurante.Saludable => "Saludable & Bowls",
                _ => "Restaurante"
            };

            huariqueRecomendado = new RestauranteItemVM(
                huariqueEntidad.Id,
                huariqueEntidad.Nombre,
                huariqueEntidad.Descripcion,
                catTexto,
                huariqueEntidad.Categoria,
                huariqueEntidad.Rango,
                huariqueEntidad.Direccion,
                huariqueEntidad.DistanciaTexto,
                huariqueEntidad.RangoPrecios,
                huariqueEntidad.HorarioAtencion,
                huariqueEntidad.TelefonoWhatsApp,
                huariqueEntidad.FotoUrl,
                huariqueEntidad.Latitud,
                huariqueEntidad.Longitud,
                huariqueEntidad.CalificacionPromedio,
                huariqueEntidad.TotalResenas,
                []
            );
        }

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
            HuariqueRecomendado = huariqueRecomendado
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
