using Microsoft.AspNetCore.Identity;
using UsmpConnect.Models;
using UsmpConnect.Data;

namespace UsmpConnect.Services;

public record ResultadoAccesoExterno(ApplicationUser? Usuario, int Estado, string? Error = null);

/// <summary>Identifica cuentas por proveedor e ID estable. Nunca las fusiona por correo.</summary>
public sealed class AccesoExternoService(UserManager<ApplicationUser> usuarios, ApplicationDbContext db)
{
    public async Task<ResultadoAccesoExterno> ResolverAsync(string proveedor, string clave, string email, string nombre, string? foto = null)
    {
        var usuario = await usuarios.FindByLoginAsync(proveedor, clave);
        if (usuario is null)
        {
            var existente = await usuarios.FindByEmailAsync(email);
            if (existente is not null)
            {
                if (await usuarios.IsLockedOutAsync(existente)) return new(null, 401, "Cuenta bloqueada temporalmente.");
                if ((await usuarios.GetRolesAsync(existente)).Count > 0 && !await usuarios.IsInRoleAsync(existente, Roles.Alumno))
                    return new(null, 403, "USMP Connect es una comunidad para alumnos.");
                return new(null, 409, "Ya existe una cuenta con ese correo. Usa su método de acceso original; no vinculamos cuentas automáticamente.");
            }
            var partes = (string.IsNullOrWhiteSpace(nombre) ? "Estudiante" : nombre.Trim()).Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            usuario = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true,
                Nombres = partes[0], Apellidos = partes.Length > 1 ? partes[1] : "", FotoUrl = foto,
                Escuela = "Por completar" };
            await using var transaccion = await db.Database.BeginTransactionAsync();
            if (!(await usuarios.CreateAsync(usuario)).Succeeded) return new(null, 400, "No pudimos crear tu cuenta. Intenta nuevamente.");
            if (!(await usuarios.AddLoginAsync(usuario, new UserLoginInfo(proveedor, clave, proveedor))).Succeeded)
                return new(null, 400, "No pudimos registrar el método de acceso.");
            await transaccion.CommitAsync();
        }
        if (await usuarios.IsLockedOutAsync(usuario)) return new(null, 401, "Cuenta bloqueada temporalmente.");
        var roles = await usuarios.GetRolesAsync(usuario);
        if (roles.Count > 0 && !roles.Contains(Roles.Alumno)) return new(null, 403, "USMP Connect es una comunidad para alumnos.");
        if (roles.Count == 0 && !(await usuarios.AddToRoleAsync(usuario, Roles.Alumno)).Succeeded)
            return new(null, 400, "No pudimos completar tu cuenta.");
        return new(usuario, 200);
    }
}
