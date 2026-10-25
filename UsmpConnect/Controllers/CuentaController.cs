using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

[AllowAnonymous]
public class CuentaController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    INotificacionService notificaciones,
    IConfiguration config,
    IFirebaseTokenValidator firebaseTokens) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await signInManager.PasswordSignInAsync(model.Email.Trim(), model.Password, model.Recordarme, lockoutOnFailure: true);
        if (result.Succeeded)
            return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");

        ModelState.AddModelError("", result.IsLockedOut
            ? "Cuenta bloqueada temporalmente por varios intentos fallidos. Intenta en unos minutos."
            : "Correo o contraseña incorrectos.");
        return View(model);
    }

    public class GoogleLoginDto
    {
        [Required, StringLength(16384)]
        public string IdToken { get; set; } = "";
    }

    [HttpPost]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto? model)
    {
        if (!ModelState.IsValid || model is null || string.IsNullOrWhiteSpace(model.IdToken))
            return BadRequest(new { error = "No recibimos la confirmación de Google. Vuelve a iniciar sesión." });

        IdentidadGoogle? identidad;
        try
        {
            identidad = await firebaseTokens.ValidarAsync(model.IdToken, HttpContext.RequestAborted);
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException && !HttpContext.RequestAborted.IsCancellationRequested)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "No pudimos verificar tu acceso con Google. Intenta nuevamente en unos minutos." });
        }
        if (identidad is null)
            return Unauthorized(new { error = "La confirmación de Google no es válida o venció. Vuelve a iniciar sesión." });

        var dominio = config["Registro:DominioCorreo"];
        if (!string.IsNullOrEmpty(dominio) && !identidad.Email.EndsWith("@" + dominio, StringComparison.OrdinalIgnoreCase))
            return StatusCode(StatusCodes.Status403Forbidden, new { error = $"Usa tu correo institucional @{dominio}." });

        const string proveedor = "Firebase";
        var user = await userManager.FindByLoginAsync(proveedor, identidad.Uid)
            ?? await userManager.FindByEmailAsync(identidad.Email);
        if (user == null)
        {
            var nombreCompleto = string.IsNullOrWhiteSpace(identidad.Nombre) ? "Estudiante" : identidad.Nombre.Trim();
            var partes = nombreCompleto.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var nombres = partes.Length > 0 ? partes[0] : "Estudiante";
            var apellidos = partes.Length > 1 ? partes[1] : "";

            user = new ApplicationUser
            {
                UserName = identidad.Email,
                Email = identidad.Email,
                EmailConfirmed = true,
                Nombres = nombres,
                Apellidos = apellidos,
                Escuela = CarrerasFia.Lista.First(c => c.Id == CarrerasFia.Sistemas).Nombre,
                FotoUrl = identidad.FotoUrl
            };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return BadRequest(new { error = "No pudimos crear tu cuenta. Intenta nuevamente." });
            }
        }
        else
        {
            if (await userManager.IsLockedOutAsync(user))
                return Unauthorized(new { error = "Cuenta bloqueada temporalmente. Intenta en unos minutos." });

            if (!string.IsNullOrEmpty(identidad.FotoUrl) && user.FotoUrl != identidad.FotoUrl)
            {
                user.FotoUrl = identidad.FotoUrl;
                if (!(await userManager.UpdateAsync(user)).Succeeded)
                    return BadRequest(new { error = "No pudimos actualizar tu cuenta. Intenta nuevamente." });
            }
        }

        var logins = await userManager.GetLoginsAsync(user);
        if (logins.Any(l => l.LoginProvider == proveedor && l.ProviderKey != identidad.Uid))
            return Unauthorized(new { error = "Este correo ya está vinculado a otra cuenta de Google." });
        if (!logins.Any(l => l.LoginProvider == proveedor && l.ProviderKey == identidad.Uid)
            && !(await userManager.AddLoginAsync(user, new UserLoginInfo(proveedor, identidad.Uid, "Google"))).Succeeded)
            return BadRequest(new { error = "No pudimos vincular tu acceso con Google. Intenta nuevamente." });

        // Repara también las cuentas Google anteriores sin rol, conservando los roles existentes.
        if ((await userManager.GetRolesAsync(user)).Count == 0
            && !(await userManager.AddToRoleAsync(user, Roles.Alumno)).Succeeded)
            return BadRequest(new { error = "No pudimos completar tu cuenta de alumno. Intenta nuevamente." });

        await signInManager.SignInAsync(user, isPersistent: true);
        return Ok(new { redirectUrl = "/" });
    }

    [HttpGet]
    public IActionResult Registro() => View(new RegistroViewModel());

    [HttpPost]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        var dominio = config["Registro:DominioCorreo"];
        if (!string.IsNullOrEmpty(dominio) && !model.Email.Trim().EndsWith("@" + dominio, StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.Email), $"Usa tu correo institucional @{dominio}");

        if (string.IsNullOrWhiteSpace(model.CodigoAlumno))
        {
            ModelState.AddModelError(nameof(model.CodigoAlumno), "Ingresa tu código de alumno");
        }

        if (!RegistroViewModel.Escuelas.Contains(model.Escuela))
            ModelState.AddModelError(nameof(model.Escuela), "Selecciona tu escuela");

        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim().ToLowerInvariant(),
            Email = model.Email.Trim().ToLowerInvariant(),
            Nombres = model.Nombres.Trim(),
            Apellidos = model.Apellidos.Trim(),
            Escuela = model.Escuela,
            CodigoAlumno = model.CodigoAlumno.Trim()
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", Traducir(e));
            return View(model);
        }

        await userManager.AddToRoleAsync(user, Roles.Alumno);
        await notificaciones.NotificarAsync(user.Id, $"¡Bienvenido a USMP Connect, {user.Nombres}! Tu cuenta de alumno está lista.", icono: "stars");
        await signInManager.SignInAsync(user, isPersistent: false);
        TempData["Toast"] = "Cuenta creada correctamente. ¡Bienvenido!";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccesoDenegado() => View();

    private static string Traducir(IdentityError e) => e.Code switch
    {
        "DuplicateUserName" or "DuplicateEmail" => "Ya existe una cuenta con ese correo.",
        "PasswordTooShort" => "La contraseña es muy corta.",
        "PasswordRequiresDigit" => "La contraseña debe tener al menos un número.",
        "PasswordRequiresLower" => "La contraseña debe tener al menos una minúscula.",
        _ => e.Description
    };
}
