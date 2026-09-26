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
        ViewData["Hoy"] = HoraPeru.Ahora.DayOfWeek;
        return View("~/Views/Horario/Index.cshtml", await Agenda.ClasesSemanaAsync(db, userId, false));
    }
}
