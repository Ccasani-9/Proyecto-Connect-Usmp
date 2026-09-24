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
    IConfiguration config) : Controller
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

    [HttpGet]
    public IActionResult Registro() => View(new RegistroViewModel());

    [HttpPost]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        var dominio = config["Registro:DominioCorreo"];
        if (!string.IsNullOrEmpty(dominio) && !model.Email.Trim().EndsWith("@" + dominio, StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.Email), $"Usa tu correo institucional @{dominio}");

        var esDocente = model.Tipo == Roles.Profesor;
        if (esDocente)
        {
            if (model.CodigoDocente != config["Registro:CodigoDocente"])
                ModelState.AddModelError(nameof(model.CodigoDocente), "Código de invitación docente inválido");
            if (string.IsNullOrWhiteSpace(model.Titulo))
                ModelState.AddModelError(nameof(model.Titulo), "Selecciona tu título");
        }
        else if (string.IsNullOrWhiteSpace(model.CodigoAlumno))
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
            Titulo = esDocente ? model.Titulo : null,
            CodigoAlumno = esDocente ? null : model.CodigoAlumno
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", Traducir(e));
            return View(model);
        }

        await userManager.AddToRoleAsync(user, esDocente ? Roles.Profesor : Roles.Alumno);
        await notificaciones.NotificarAsync(user.Id, $"¡Bienvenido a USMP Connect, {user.Nombres}! Tu cuenta de {(esDocente ? "docente" : "alumno")} está lista.", icono: "stars");
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
