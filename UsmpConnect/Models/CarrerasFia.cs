namespace UsmpConnect.Models;

/// <summary>
/// Carreras oficiales de la Facultad de Ingeniería y Arquitectura (FIA) - USMP.
/// </summary>
public static class CarrerasFia
{
    public const string Sistemas = "sistemas";
    public const string Civil = "civil";
    public const string Industrial = "industrial";
    public const string Arquitectura = "arquitectura";
    public const string Aeronauticas = "aeronauticas";
    public const string Ciberseguridad = "ciberseguridad";
    public const string Datos = "datos";
    public const string Ia = "ia";

    public record CarreraItem(string Id, string Nombre, string NombreCorto, string Icono);

    public static readonly List<CarreraItem> Lista =
    [
        new(Sistemas, "Ingeniería de Computación y Sistemas", "Ing. Sistemas", "bi-laptop"),
        new(Civil, "Ingeniería Civil", "Ing. Civil", "bi-cone-striped"),
        new(Industrial, "Ingeniería Industrial", "Ing. Industrial", "bi-gear-wide-connected"),
        new(Arquitectura, "Arquitectura", "Arquitectura", "bi-building"),
        new(Aeronauticas, "Ciencias Aeronáuticas", "Cs. Aeronáuticas", "bi-airplane-fill"),
        new(Ciberseguridad, "Ingeniería en Ciberseguridad y Análisis Forense Digital", "Ciberseguridad", "bi-shield-lock-fill"),
        new(Datos, "Ingeniería en Ciencias de Datos", "Ciencia de Datos", "bi-database-fill-gear"),
        new(Ia, "Ingeniería en Inteligencia Artificial y Robótica", "IA y Robótica", "bi-cpu-fill")
    ];

    public static string Nombre(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "Ingeniería de Computación y Sistemas";

        return Lista.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase))?.Nombre
            ?? "Ingeniería de Computación y Sistemas";
    }

    public static CarreraItem Item(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Lista[0];

        return Lista.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) ?? Lista[0];
    }

    public static string NombreCorto(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "Ing. Sistemas";

        return Lista.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase))?.NombreCorto
            ?? "Ing. Sistemas";
    }

    public static string? MapearEscuela(string? escuela)
    {
        if (string.IsNullOrWhiteSpace(escuela)) return null;
        var lower = escuela.ToLowerInvariant();
        if (lower.Contains("sistema") || lower.Contains("computación") || lower.Contains("computacion")) return Sistemas;
        if (lower.Contains("civil")) return Civil;
        if (lower.Contains("industrial")) return Industrial;
        if (lower.Contains("arquitectura")) return Arquitectura;
        if (lower.Contains("aeronáutica") || lower.Contains("aeronautica")) return Aeronauticas;
        if (lower.Contains("ciberseguridad")) return Ciberseguridad;
        if (lower.Contains("datos")) return Datos;
        if (lower.Contains("ia") || lower.Contains("inteligencia artificial") || lower.Contains("robótica") || lower.Contains("robotica")) return Ia;
        return null;
    }

    /// <summary>
    /// Determina la etiqueta visible para la tarjeta del curso (Tronco Común o la Carrera principal).
    /// </summary>
    public static string EtiquetaCurso(string codigo) => codigo switch
    {
        "HU-101" or "MA-101" or "MA-102" or "FI-102" => "Tronco Común FIA",
        "CI-201" or "CI-301" => "Ing. Civil",
        "IN-201" or "IN-301" => "Ing. Industrial",
        "AR-101" => "Arquitectura",
        "AE-101" => "Ciencias Aeronáuticas",
        "CB-401" => "Ciberseguridad",
        "CD-401" => "Ciencia de Datos",
        "IA-401" => "IA y Robótica",
        _ => "Ing. Sistemas"
    };

    /// <summary>
    /// Verifica si un curso aplica al filtro de carrera seleccionado.
    /// Cursos de tronco común aplican a las carreras de ingeniería.
    /// </summary>
    public static bool AplicaACarrera(string codigo, string? carreraId)
    {
        var id = string.IsNullOrWhiteSpace(carreraId) ? Sistemas : carreraId.ToLowerInvariant();

        // Cursos de ciencias básicas / tronco común aplican a todas las ingenierías
        var esTroncoComun = codigo is "HU-101" or "MA-101" or "MA-102" or "FI-102";
        if (esTroncoComun)
        {
            if (id == Arquitectura) return codigo == "HU-101";
            return true;
        }

        return id switch
        {
            Sistemas => codigo.StartsWith("CS-") || codigo.StartsWith("IS-") || codigo is "MA-201",
            Civil => codigo.StartsWith("CI-"),
            Industrial => codigo.StartsWith("IN-") || codigo is "CS-101" or "CS-301",
            Arquitectura => codigo.StartsWith("AR-"),
            Aeronauticas => codigo.StartsWith("AE-"),
            Ciberseguridad => codigo.StartsWith("CB-") || codigo is "CS-101" or "CS-201" or "CS-401" or "CS-402",
            Datos => codigo.StartsWith("CD-") || codigo is "CS-101" or "CS-201" or "CS-301" or "CS-501",
            Ia => codigo.StartsWith("IA-") || codigo is "CS-101" or "CS-201" or "CS-501",
            _ => codigo.StartsWith("CS-") || codigo.StartsWith("IS-")
        };
    }
}
