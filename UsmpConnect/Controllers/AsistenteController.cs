using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsmpConnect.Services;
using UsmpConnect.Services.Agente;

namespace UsmpConnect.Controllers;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AsistenteController(IAsistenteService asistente, ConversacionesActivas activas,
    ILogger<AsistenteController> logger) : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private string Clave => "AsistenteHistorial:" + User.GetUserId();
    private string ClaveActiva => HttpContext.Session.Id + ":" + User.GetUserId();

    private List<MensajeAsistente> LeerHistorial() => JsonSerializer.Deserialize<List<MensajeAsistente>>(
        HttpContext.Session.GetString(Clave) ?? "[]", JsonOptions) ?? [];

    [HttpGet]
    public async Task<IActionResult> Historial(CancellationToken ct)
    {
        await HttpContext.Session.LoadAsync(ct);
        return Json(new { configurado = asistente.EstaConfigurado, mensajes = LeerHistorial() });
    }

    [HttpPost]
    [RequestSizeLimit(8192)]
    public async Task<IActionResult> Limpiar(CancellationToken ct)
    {
        await HttpContext.Session.LoadAsync(ct);
        var claveActiva = ClaveActiva;
        if (!activas.IntentarAbrir(claveActiva)) return Conflict(new { error = "Detén la respuesta antes de iniciar otra conversación." });
        try
        {
            HttpContext.Session.Remove(Clave);
            await HttpContext.Session.CommitAsync(ct);
            return NoContent();
        }
        finally { activas.Cerrar(claveActiva); }
    }

    [HttpPost]
    [RequestSizeLimit(8192)]
    public async Task<IActionResult> Preguntar(string? mensaje, CancellationToken ct)
    {
        mensaje = mensaje?.Trim();
        if (string.IsNullOrWhiteSpace(mensaje) || mensaje.Length > 1000)
            return BadRequest(new { error = "Escribe un mensaje de entre 1 y 1.000 caracteres." });
        if (!asistente.EstaConfigurado)
            return StatusCode(503, new { error = "El asistente todavía no está disponible. Puedes utilizar el buscador de la plataforma." });
        await HttpContext.Session.LoadAsync(ct);
        // Establece la cookie de sesión ANTES de comenzar el flujo de respuesta.
        HttpContext.Session.SetString("AsistenteActivo", "1");
        var claveActiva = ClaveActiva;
        if (!activas.IntentarAbrir(claveActiva)) return Conflict(new { error = "Ya hay una respuesta en curso." });
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limite.CancelAfter(TimeSpan.FromSeconds(60));
        Response.ContentType = "application/x-ndjson; charset=utf-8";
        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-Accel-Buffering"] = "no";
        var historial = LeerHistorial();
        var contexto = new ContextoAsistente();
        var texto = new StringBuilder();
        try
        {
            await foreach (var parte in asistente.ResponderAsync(mensaje, historial, contexto, limite.Token))
            {
                if (texto.Length + parte.Length > 12_000) throw new InvalidOperationException("Respuesta demasiado extensa.");
                texto.Append(parte);
                await EnviarAsync(new { tipo = "texto", texto = parte }, ct);
            }
            if (texto.Length == 0) throw new InvalidOperationException("Respuesta vacía.");
            historial.Add(new("usuario", mensaje));
            historial.Add(new("asistente", texto.ToString(), contexto.Tarjetas, contexto.Fuentes));
            HttpContext.Session.SetString(Clave, JsonSerializer.Serialize(historial.TakeLast(12).ToList(), JsonOptions));
            await HttpContext.Session.CommitAsync(limite.Token);
            await EnviarAsync(new { tipo = "fin", tarjetas = contexto.Tarjetas, fuentes = contexto.Fuentes }, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            logger.LogWarning("Respuesta del asistente interrumpida ({Tipo}).", ex.GetType().Name);
            if (!ct.IsCancellationRequested)
                await EnviarAsync(new { tipo = "error", error = "No se pudo completar la respuesta. Inténtalo de nuevo en unos momentos." }, ct);
        }
        finally { activas.Cerrar(claveActiva); }
        return new EmptyResult();
    }

    private async Task EnviarAsync(object evento, CancellationToken ct)
    {
        await Response.WriteAsync(JsonSerializer.Serialize(evento, JsonOptions) + "\n", ct);
        await Response.Body.FlushAsync(ct);
    }
}
