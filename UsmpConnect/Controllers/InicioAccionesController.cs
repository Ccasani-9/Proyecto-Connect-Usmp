using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;

namespace UsmpConnect.Controllers;

/// <summary>Mis Horarios → "Ver todo": horario semanal completo.</summary>
public class HorarioController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var clases = await Agenda.ClasesSemanaAsync(db, User.GetUserId(), User.EsProfesor());
        ViewData["Hoy"] = HoraPeru.Hoy.DayOfWeek;
        return View(clases);
    }
}

/// <summary>Recordatorios personales del calendario.</summary>
public class EventosController(ApplicationDbContext db) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Crear(string titulo, DateOnly fecha, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(titulo) || titulo.Length > 120)
        {
            TempData["ToastError"] = "Escribe un título (máx. 120 caracteres).";
        }
        else
        {
            db.Eventos.Add(new EventoAcademico { Titulo = titulo.Trim(), Fecha = fecha, Tipo = TipoEvento.Personal, UsuarioId = User.GetUserId() });
            await db.SaveChangesAsync();
            TempData["Toast"] = $"Recordatorio agregado para el {fecha.ToString("d 'de' MMMM", HoraPeru.Cultura)}.";
        }
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id, string? returnUrl)
    {
        var userId = User.GetUserId();
        await db.Eventos.Where(e => e.Id == id && e.UsuarioId == userId).ExecuteDeleteAsync();
        TempData["Toast"] = "Recordatorio eliminado.";
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }
}

/// <summary>Banner "¿Tuviste algún inconveniente con tu matrícula?".</summary>
public class CasosController(ApplicationDbContext db, INotificacionService notificaciones) : Controller
{
    public static readonly string[] Tipos = ["Cruce de horarios", "Curso no aparece en mi matrícula", "Pago no reflejado", "Sección sin vacantes", "Otro"];

    [HttpPost]
    public async Task<IActionResult> Crear(string tipo, string descripcion)
    {
        if (!Tipos.Contains(tipo) || string.IsNullOrWhiteSpace(descripcion) || descripcion.Length > 1000)
        {
            TempData["ToastError"] = "Completa el tipo de caso y una descripción (máx. 1000 caracteres).";
            return RedirectToAction("Index", "Home");
        }

        var caso = new CasoMatricula { UsuarioId = User.GetUserId(), Tipo = tipo, Descripcion = descripcion.Trim() };
        db.CasosMatricula.Add(caso);
        await db.SaveChangesAsync();

        await notificaciones.NotificarAsync(caso.UsuarioId,
            $"Registramos tu caso #{caso.Id:D4} ({tipo}). Un asesor te contactará pronto.", icono: "clipboard-check");
        TempData["Toast"] = $"Caso #{caso.Id:D4} registrado. Te avisaremos por Noticias.";
        return RedirectToAction("Index", "Home");
    }
}

public class NotificacionesController(ApplicationDbContext db) : Controller
{
    [HttpPost]
    public async Task<IActionResult> MarcarLeidas()
    {
        var userId = User.GetUserId();
        await db.Notificaciones
            .Where(n => n.UsuarioId == userId && !n.Leida)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));
        return NoContent();
    }
}
