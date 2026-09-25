using System.Text.RegularExpressions;

namespace UsmpConnect.Services;

/// <summary>Enlaces wa.me para contactar a otro alumno por WhatsApp (celulares de Perú).</summary>
public static partial class WhatsApp
{
    /// <summary>9 dígitos empezando con 9, sin el +51.</summary>
    public const string Patron = "^9[0-9]{8}$";

    public static bool EsValido(string? numero) => numero is not null && NumeroRegex().IsMatch(numero);

    public static string Enlace(string numero, string mensaje) =>
        $"https://wa.me/51{numero}?text={Uri.EscapeDataString(mensaje)}";

    [GeneratedRegex(Patron)]
    private static partial Regex NumeroRegex();
}
