using Microsoft.AspNetCore.Identity;

namespace UsmpConnect.Models;

/// <summary>
/// Usuario de la plataforma. Puede ser Alumno o Profesor (ver <see cref="Roles"/>).
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";

    /// <summary>"Ing.", "Lic.", "Dr." — solo para docentes.</summary>
    public string? Titulo { get; set; }

    /// <summary>Código universitario — solo para alumnos.</summary>
    public string? CodigoAlumno { get; set; }

    public string Escuela { get; set; } = "";

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();

    /// <summary>"María G." — como se muestra en tips, apuntes y publicaciones.</summary>
    public string NombreCorto =>
        string.IsNullOrWhiteSpace(Apellidos) ? Nombres : $"{PrimeraPalabra(Nombres)} {Apellidos[0]}.";

    /// <summary>"Ing. Ramírez" — como se muestra a un docente.</summary>
    public string NombreDocente => $"{Titulo} {PrimeraPalabra(Apellidos)}".Trim();

    public string Iniciales =>
        $"{(Nombres.Length > 0 ? Nombres[0] : ' ')}{(Apellidos.Length > 0 ? Apellidos[0] : ' ')}".Trim().ToUpperInvariant();

    private static string PrimeraPalabra(string texto) => texto.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
}

public static class Roles
{
    public const string Alumno = "Alumno";
    public const string Profesor = "Profesor";
}
