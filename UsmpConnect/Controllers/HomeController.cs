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
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var esProfesor = User.EsProfesor();
        var hoy = HoraPeru.Hoy;
        var hora = HoraPeru.Ahora.Hour;

        var semana = await Agenda.ClasesSemanaAsync(db, userId, esProfesor);
        var eventos = await Agenda.EventosAsync(db, userId);

        var vm = new InicioViewModel
        {
            PrimerNombre = User.GetNombre().Split(' ').Skip(esProfesor ? 1 : 0).FirstOrDefault() ?? "",
            Saludo = hora < 12 ? "Buenos días" : hora < 19 ? "Buenas tardes" : "Buenas noches",
            EsProfesor = esProfesor,
            Hoy = Agenda.Dia(semana, hoy, "HOY"),
            Manana = Agenda.Dia(semana, hoy.AddDays(1), "MAÑANA"),
            Eventos = eventos,
            ProximosEventos = eventos.Where(e => DateOnly.Parse(e.Fecha) >= hoy).Take(4).ToList(),
            FechaHoy = hoy
        };

        if (esProfesor)
        {
            var secciones = await db.Secciones.AsNoTracking()
                .Where(s => s.DocenteId == userId)
                .Select(s => new
                {
                    s.Id, Curso = s.Curso.Nombre, s.Codigo,
                    Alumnos = s.Matriculas.Count,
                    ConNotas = s.Matriculas.Count(m => m.Nota != null && m.Nota.EP != null),
                    Horarios = s.Horarios.Select(h => new { h.Dia, h.HoraInicio, h.HoraFin }).ToList()
                })
                .ToListAsync();

            vm.Secciones = secciones
                .OrderBy(s => s.Curso)
                .Select(s => new SeccionResumenVM(s.Id, s.Curso, s.Codigo, s.Alumnos, s.ConNotas,
                    string.Join(" · ", s.Horarios.Select(h => $"{HoraPeru.DiaCorto(h.Dia)} {h.HoraInicio:HH\\:mm}-{h.HoraFin:HH\\:mm}"))))
                .ToList();
        }
        else
        {
            vm.Notas = await NotasController.NotasAlumnoAsync(db, userId);
        }

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
