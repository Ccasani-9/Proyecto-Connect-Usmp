using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsmpConnect.Data;
using UsmpConnect.Services;

namespace UsmpConnect.Controllers;

[AllowAnonymous]
public class BuscarController(ApplicationDbContext db, IBuscadorAlgolia buscador) : Controller
{
    public async Task<IActionResult> Index(string? q, CancellationToken cancellationToken)
    {
        var texto = (q ?? "").Trim();
        if (texto.Length > 100) texto = texto[..100];
        ViewData["BusquedaGlobal"] = texto;
        return View(await buscador.BuscarAsync(texto, db, cancellationToken));
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Sugerencias(string? q, CancellationToken cancellationToken)
    {
        if (q?.Trim().Length is not (>= 2 and <= 100)) return Json(Array.Empty<AlgoliaItem>());
        return Json(await buscador.BuscarAsync(q, db, cancellationToken));
    }
}
