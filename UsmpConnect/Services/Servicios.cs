using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using UsmpConnect.Data;
using UsmpConnect.Models;

namespace UsmpConnect.Services;

/// <summary>Hora de Lima (UTC-5, sin horario de verano). Render corre en UTC.</summary>
public static class HoraPeru
{
    public static readonly CultureInfo Cultura = new("es-PE");

    public static DateTime Ahora => DateTime.UtcNow.AddHours(-5);
    public static DateOnly Hoy => DateOnly.FromDateTime(Ahora);

    public static DateTime DesdeUtc(DateTime utc) => utc.AddHours(-5);

    /// <summary>"hace 2 días", "hace 5 min"...</summary>
    public static string Hace(DateTime utc)
    {
        var d = DateTime.UtcNow - utc;
        if (d.TotalMinutes < 1) return "justo ahora";
        if (d.TotalMinutes < 60) return $"hace {(int)d.TotalMinutes} min";
        if (d.TotalHours < 24) return $"hace {(int)d.TotalHours} h";
        if (d.TotalDays < 2) return "ayer";
        if (d.TotalDays < 7) return $"hace {(int)d.TotalDays} días";
        if (d.TotalDays < 14) return "hace 1 semana";
        if (d.TotalDays < 30) return $"hace {(int)(d.TotalDays / 7)} semanas";
        return DesdeUtc(utc).ToString("d MMM yyyy", Cultura);
    }

    public static string DiaCorto(DayOfWeek dia) => dia switch
    {
        DayOfWeek.Monday => "LUN", DayOfWeek.Tuesday => "MAR", DayOfWeek.Wednesday => "MIÉ",
        DayOfWeek.Thursday => "JUE", DayOfWeek.Friday => "VIE", DayOfWeek.Saturday => "SÁB", _ => "DOM"
    };

    public static string DiaLargo(DayOfWeek dia) => Cultura.TextInfo.ToTitleCase(Cultura.DateTimeFormat.GetDayName(dia));
}

public static class ClaimsPrincipalExtensions
{
    public const string NombreClaim = "usmp:nombre";
    public const string EscuelaClaim = "usmp:escuela";
    public const string InicialesClaim = "usmp:iniciales";

    public static string GetUserId(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    public static string GetNombre(this ClaimsPrincipal user) => user.FindFirstValue(NombreClaim) ?? user.Identity?.Name ?? "";
    public static string GetEscuela(this ClaimsPrincipal user) => user.FindFirstValue(EscuelaClaim) ?? "";
    public static string GetIniciales(this ClaimsPrincipal user) => user.FindFirstValue(InicialesClaim) ?? "?";
    public static bool EsProfesor(this ClaimsPrincipal user) => user.IsInRole(Roles.Profesor);
}

/// <summary>Agrega nombre, escuela e iniciales a la cookie para mostrarlos en la barra superior sin ir a la BD.</summary>
public class UsmpClaimsFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var esDocente = !string.IsNullOrEmpty(user.Titulo);
        identity.AddClaim(new Claim(ClaimsPrincipalExtensions.NombreClaim,
            esDocente ? $"{user.Titulo} {user.NombreCompleto}" : user.NombreCompleto));
        identity.AddClaim(new Claim(ClaimsPrincipalExtensions.EscuelaClaim, user.Escuela));
        identity.AddClaim(new Claim(ClaimsPrincipalExtensions.InicialesClaim, user.Iniciales));
        return identity;
    }
}

public interface INotificacionService
{
    Task NotificarAsync(string usuarioId, string mensaje, string? url = null, string icono = "bell");
}

/// <summary>
/// Guarda notificaciones en la BD (campanita "Noticias" de la barra superior).
/// Siguiente entrega: además publicarlas en tiempo real por WebSocket (PieSocket).
/// </summary>
public class NotificacionService(ApplicationDbContext db) : INotificacionService
{
    public async Task NotificarAsync(string usuarioId, string mensaje, string? url = null, string icono = "bell")
    {
        db.Notificaciones.Add(new Notificacion { UsuarioId = usuarioId, Mensaje = mensaje, Url = url, Icono = icono });
        await db.SaveChangesAsync();
    }
}

public record ArchivoGuardado(string Url, string NombreOriginal, long Bytes, string Extension);

public interface IAlmacenArchivos
{
    /// <summary>
    /// Valida y guarda un archivo subido en <c>wwwroot/uploads/{carpeta}</c>.
    /// Lanza <see cref="ArchivoInvalidoException"/> si la extensión o el tamaño no son válidos.
    /// </summary>
    Task<ArchivoGuardado> GuardarAsync(IFormFile archivo, string carpeta, string[] extensionesPermitidas, long maxBytes);

    /// <summary>Guarda bytes generados por el servidor (útil para datos de prueba).</summary>
    Task<string> GuardarBytesAsync(byte[] contenido, string carpeta, string extension);

    string RutaFisica(string url);
}

public class ArchivoInvalidoException(string mensaje) : Exception(mensaje);

public class AlmacenArchivosLocal(IWebHostEnvironment env) : IAlmacenArchivos
{
    public static readonly string[] Imagenes = [".jpg", ".jpeg", ".png", ".webp"];
    public static readonly string[] Documentos = [".pdf", ".doc", ".docx"];

    public async Task<ArchivoGuardado> GuardarAsync(IFormFile archivo, string carpeta, string[] extensionesPermitidas, long maxBytes)
    {
        if (archivo.Length == 0)
            throw new ArchivoInvalidoException("El archivo está vacío.");
        if (archivo.Length > maxBytes)
            throw new ArchivoInvalidoException($"El archivo supera el máximo de {maxBytes / (1024 * 1024)} MB.");

        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!extensionesPermitidas.Contains(ext))
            throw new ArchivoInvalidoException($"Formato no permitido. Usa: {string.Join(", ", extensionesPermitidas)}.");

        var destino = Directorio(carpeta);
        var nombre = $"{Guid.NewGuid():N}{ext}";
        await using (var fs = File.Create(Path.Combine(destino, nombre)))
            await archivo.CopyToAsync(fs);

        return new ArchivoGuardado($"/uploads/{carpeta}/{nombre}", Path.GetFileName(archivo.FileName), archivo.Length, ext);
    }

    public async Task<string> GuardarBytesAsync(byte[] contenido, string carpeta, string extension)
    {
        var nombre = $"{Guid.NewGuid():N}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(Directorio(carpeta), nombre), contenido);
        return $"/uploads/{carpeta}/{nombre}";
    }

    public string RutaFisica(string url) =>
        Path.Combine(env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

    private string Directorio(string carpeta)
    {
        if (carpeta.Contains("..") || carpeta.Contains('/') || carpeta.Contains('\\'))
            throw new ArgumentException("Carpeta inválida", nameof(carpeta));
        var dir = Path.Combine(env.WebRootPath, "uploads", carpeta);
        Directory.CreateDirectory(dir);
        return dir;
    }
}
