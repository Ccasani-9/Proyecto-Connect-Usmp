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
        var clases = await Agenda.ClasesSemanaAsync(db, User.GetUserId(), false);
        ViewData["Hoy"] = HoraPeru.Hoy.DayOfWeek;
        return View(clases);
    }
}

/// <summary>Recordatorios personales del calendario.</summary>
public class EventosController(ApplicationDbContext db) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Crear(string titulo, DateOnly fecha, string? returnUrl,
        string? emoji = null, bool semanal = false, DateOnly? hasta = null, int[]? dias = null)
    {
        try
        {
            if (!ModelState.IsValid) throw new ArgumentException("Revisa las fechas y los días seleccionados.");
            var prefijo = RepeticionActividad.Emojis.Contains(emoji) ? emoji + " " : "";
            if (string.IsNullOrWhiteSpace(titulo) || prefijo.Length + titulo.Trim().Length > 120)
                throw new ArgumentException("Escribe un título de hasta 110 caracteres para dejar espacio al emoji.");
            var fechas = RepeticionActividad.Fechas(fecha, semanal, hasta, dias);
            var userId = User.GetUserId();
            db.Eventos.AddRange(fechas.Select(dia => new EventoAcademico
            {
                Titulo = prefijo + titulo.Trim(), Fecha = dia, Tipo = TipoEvento.Personal, UsuarioId = userId
            }));
            await db.SaveChangesAsync();
            TempData["Toast"] = fechas.Count == 1 ? "Actividad guardada." : $"Se guardaron {fechas.Count} actividades. Puedes eliminar cada fecha por separado.";
        }
        catch (ArgumentException ex) { TempData["ToastError"] = ex.Message; }
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Calendario");
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
    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Resumen() => ViewComponent("Notificaciones");

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
