using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UsmpConnect.Services;

public static class EnumExtensions
{
    /// <summary>Texto de [Display(Name = "...")] o, si no tiene, el nombre del valor.</summary>
    public static string Nombre(this Enum valor) =>
        valor.GetType().GetField(valor.ToString())?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? valor.ToString();

    /// <summary>Opciones para un &lt;select&gt; a partir de un enum.</summary>
    public static IEnumerable<SelectListItem> Opciones<T>() where T : struct, Enum =>
        Enum.GetValues<T>().Select(v => new SelectListItem(v.Nombre(), v.ToString()));
}
