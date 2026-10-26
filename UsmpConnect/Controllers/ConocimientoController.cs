using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsmpConnect.Services.Conocimiento;

namespace UsmpConnect.Controllers;

/// <summary>Consulta de diagnóstico; la interfaz de alumnos será el asistente de la siguiente rama.</summary>
[Authorize]
public sealed class ConocimientoController(IKnowledgeBaseService conocimiento) : Controller
{
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Buscar(string? consulta, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(consulta) || consulta.Trim().Length is < 3 or > 500)
            return BadRequest(new { error = "Escribe una consulta de entre 3 y 500 caracteres." });
        return Json(await conocimiento.BuscarAsync(consulta, ct: ct));
    }
}
