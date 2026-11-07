using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UsmpConnect.Services;

/// <summary>Un formulario abierto antes de un reinicio recibe instrucciones para obtener un token nuevo.</summary>
public sealed class RecuperarFormularioAccesoFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not IAntiforgeryValidationFailedResult ||
            !string.Equals(context.RouteData.Values["controller"]?.ToString(), "Cuenta", StringComparison.OrdinalIgnoreCase))
            return;

        if (string.Equals(context.RouteData.Values["action"]?.ToString(), "GoogleLogin", StringComparison.OrdinalIgnoreCase))
            context.Result = new BadRequestObjectResult(new
            {
                error = "El formulario de acceso venció. Recarga esta página y vuelve a continuar con Google.",
                recargar = true
            });
        else if (string.Equals(context.RouteData.Values["action"]?.ToString(), "Logout", StringComparison.OrdinalIgnoreCase))
        {
            if (context.Controller is Controller controller)
                controller.TempData["ToastError"] = "El formulario de salida venció. Ya lo renovamos. Vuelve a pulsar Cerrar sesión.";
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
        else
            context.Result = new RedirectToActionResult("Login", "Cuenta", new { aviso = "formulario-vencido" });
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}
