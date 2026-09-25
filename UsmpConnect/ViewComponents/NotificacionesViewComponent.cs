using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Services;

namespace UsmpConnect.ViewComponents;

/// <summary>Campanita "NOTICIAS" de la barra superior.</summary>
public class NotificacionesViewComponent(ApplicationDbContext db) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = UserClaimsPrincipal.GetUserId();
        var lista = await db.Notificaciones.AsNoTracking()
            .Where(n => n.UsuarioId == userId)
            .OrderByDescending(n => n.Fecha)
            .Take(8)
            .ToListAsync();
        ViewData["NoLeidas"] = await db.Notificaciones.CountAsync(n => n.UsuarioId == userId && !n.Leida);
        return View(lista);
    }
}
