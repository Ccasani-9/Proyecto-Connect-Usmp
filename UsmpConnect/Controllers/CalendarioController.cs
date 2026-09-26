using Microsoft.AspNetCore.Mvc;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

public class CalendarioController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var clases = await Agenda.ClasesSemanaAsync(db, userId, false);
        var eventos = await Agenda.EventosAsync(db, userId);

        var vm = new CalendarioViewModel
        {
            Clases = clases,
            Eventos = eventos,
            FechaHoy = HoraPeru.Hoy
        };

        return View(vm);
    }
}
