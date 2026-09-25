using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

/// <summary>
/// Lost &amp; Found: objetos encontrados que esperan a su dueño (con reclamos que revisa quien lo reportó)
/// y objetos que los alumnos están buscando.
/// </summary>
public class LostFoundController(ApplicationDbContext db, IAlmacenArchivos almacen, INotificacionService notificaciones) : Controller
{
    private const long MaxFoto = 5 * 1024 * 1024;

    public async Task<IActionResult> Index(TipoReporte tipo = TipoReporte.Encontrado)
    {
        var userId = User.GetUserId();

        var objetos = await db.ObjetosPerdidos.AsNoTracking()
            .Include(o => o.Reportante)
            .Include(o => o.Reclamos.Where(r => r.Estado == EstadoReclamo.Pendiente))
            .Where(o => o.Tipo == tipo)
            .OrderBy(o => o.Estado)
            .ThenByDescending(o => o.FechaReporte)
            .ToListAsync();

        // Puede no existir si la sesión es de una BD anterior (la BD se recrea en cada arranque).
        var yo = await db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.CodigoAlumno, u.Nombres, u.Apellidos })
            .FirstOrDefaultAsync();

        return View(new LostFoundIndexViewModel
        {
            Tipo = tipo,
            Objetos = objetos,
            UsuarioId = userId,
            MiCodigo = yo?.CodigoAlumno ?? "",
            MiNombre = yo is null ? "" : $"{yo.Nombres} {yo.Apellidos}".Trim(),
            MisReclamos = objetos.Where(o => o.Reclamos.Any(r => r.UsuarioId == userId)).Select(o => o.Id).ToHashSet()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Reportar(ReportarObjetoVM vm)
    {
        if (!ObjetoPerdido.Pabellones.Contains(vm.Pabellon))
            ModelState.AddModelError(nameof(vm.Pabellon), "Selecciona un pabellón de la lista.");
        if (vm.FechaHallazgo > HoraPeru.Ahora.AddMinutes(5))
            ModelState.AddModelError(nameof(vm.FechaHallazgo), "La fecha no puede estar en el futuro.");
        if (vm.Tipo == TipoReporte.Encontrado && vm.Custodia is null)
            ModelState.AddModelError(nameof(vm.Custodia), "Indica dónde quedó el objeto.");
        if (vm.Tipo == TipoReporte.Perdido && string.IsNullOrEmpty(vm.WhatsApp))
            ModelState.AddModelError(nameof(vm.WhatsApp), "Ingresa tu WhatsApp para que te contacten si lo encuentran.");

        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = PrimerError();
            return RedirectToAction(nameof(Index), new { tipo = vm.Tipo });
        }

        string? foto = null;
        if (vm.Foto is { Length: > 0 })
        {
            try
            {
                foto = (await almacen.GuardarAsync(vm.Foto, "lostfound", AlmacenArchivosLocal.Imagenes, MaxFoto)).Url;
            }
            catch (ArchivoInvalidoException ex)
            {
                TempData["ToastError"] = ex.Message;
                return RedirectToAction(nameof(Index), new { tipo = vm.Tipo });
            }
        }

        db.ObjetosPerdidos.Add(new ObjetoPerdido
        {
            Tipo = vm.Tipo,
            ReportanteId = User.GetUserId(),
            Titulo = vm.Titulo.Trim(),
            Categoria = vm.Categoria!.Value,
            Pabellon = vm.Pabellon,
            Lugar = vm.Lugar?.Trim() ?? "",
            FechaHallazgo = vm.FechaHallazgo!.Value,
            FotoUrl = foto,
            Custodia = vm.Tipo == TipoReporte.Encontrado ? vm.Custodia : null,
            WhatsApp = string.IsNullOrEmpty(vm.WhatsApp) ? null : vm.WhatsApp,
            Descripcion = string.IsNullOrWhiteSpace(vm.Descripcion) ? null : vm.Descripcion.Trim()
        });
        await db.SaveChangesAsync();

        TempData["Toast"] = vm.Tipo == TipoReporte.Encontrado
            ? "¡Gracias! Publicamos el objeto para que su dueño lo encuentre."
            : "Publicamos tu objeto perdido. Te escribirán por WhatsApp si alguien lo encuentra.";
        return RedirectToAction(nameof(Index), new { tipo = vm.Tipo });
    }

    [HttpPost]
    public async Task<IActionResult> Reclamar(ReclamarObjetoVM vm)
    {
        var userId = User.GetUserId();
        var objeto = await db.ObjetosPerdidos.FirstOrDefaultAsync(o => o.Id == vm.ObjetoId && o.Tipo == TipoReporte.Encontrado);
        if (objeto is null)
            return NotFound();

        if (objeto.ReportanteId == userId)
            ModelState.AddModelError("", "No puedes reclamar un objeto que tú reportaste.");
        else if (objeto.Estado == EstadoObjeto.Devuelto)
            ModelState.AddModelError("", "Este objeto ya fue entregado a su dueño.");
        else if (await db.ReclamosObjetos.AnyAsync(r => r.ObjetoId == objeto.Id && r.UsuarioId == userId && r.Estado == EstadoReclamo.Pendiente))
            ModelState.AddModelError("", "Ya enviaste un reclamo por este objeto. Espera la respuesta.");

        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = PrimerError();
            return RedirectToAction(nameof(Index));
        }

        db.ReclamosObjetos.Add(new ReclamoObjeto
        {
            ObjetoId = objeto.Id,
            UsuarioId = userId,
            CodigoAlumno = vm.CodigoAlumno,
            NombreCompleto = vm.NombreCompleto.Trim(),
            Descripcion = vm.Descripcion.Trim(),
            WhatsApp = vm.WhatsApp
        });
        await db.SaveChangesAsync();

        await notificaciones.NotificarAsync(objeto.ReportanteId,
            $"{User.GetNombre()} dice que “{objeto.Titulo}” es suyo. Revisa su solicitud.",
            $"/LostFound/Reclamos/{objeto.Id}", "person-raised-hand");

        TempData["Toast"] = "Solicitud enviada. Te avisaremos en Noticias cuando la revisen.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Quien reportó el objeto revisa las solicitudes de reclamo.</summary>
    public async Task<IActionResult> Reclamos(int id)
    {
        var userId = User.GetUserId();
        var objeto = await db.ObjetosPerdidos.AsNoTracking()
            .Include(o => o.Reclamos.OrderBy(r => r.Estado).ThenByDescending(r => r.Fecha))
            .FirstOrDefaultAsync(o => o.Id == id && o.ReportanteId == userId && o.Tipo == TipoReporte.Encontrado);
        return objeto is null ? NotFound() : View(objeto);
    }

    [HttpPost]
    public async Task<IActionResult> ResolverReclamo(int id, bool aprobar)
    {
        var userId = User.GetUserId();
        var reclamo = await db.ReclamosObjetos
            .Include(r => r.Objeto).ThenInclude(o => o.Reclamos)
            .FirstOrDefaultAsync(r => r.Id == id && r.Objeto.ReportanteId == userId);
        if (reclamo is null)
            return NotFound();
        if (reclamo.Estado != EstadoReclamo.Pendiente || reclamo.Objeto.Estado == EstadoObjeto.Devuelto)
        {
            TempData["ToastError"] = "Esta solicitud ya fue resuelta.";
            return RedirectToAction(nameof(Reclamos), new { id = reclamo.ObjetoId });
        }

        var objeto = reclamo.Objeto;
        var rechazados = new List<ReclamoObjeto>();

        if (aprobar)
        {
            reclamo.Estado = EstadoReclamo.Aprobado;
            objeto.Estado = EstadoObjeto.Devuelto;
            // Solo puede haber un dueño: las demás solicitudes se rechazan.
            rechazados.AddRange(objeto.Reclamos.Where(r => r.Id != reclamo.Id && r.Estado == EstadoReclamo.Pendiente));
        }
        else
        {
            rechazados.Add(reclamo);
        }
        rechazados.ForEach(r => r.Estado = EstadoReclamo.Rechazado);
        await db.SaveChangesAsync();

        if (aprobar)
        {
            var dondeRecoger = objeto.Custodia == CustodiaObjeto.Reportante && objeto.WhatsApp is not null
                ? $"Escríbele a {User.GetNombre()} al WhatsApp {objeto.WhatsApp} para recogerlo."
                : $"Recógelo: {objeto.Custodia?.Nombre().ToLower() ?? "coordina con quien lo encontró"}.";
            await notificaciones.NotificarAsync(reclamo.UsuarioId,
                $"¡Aprobaron tu reclamo de “{objeto.Titulo}”! {dondeRecoger}", "/LostFound", "check2-circle");
        }
        foreach (var r in rechazados)
        {
            await notificaciones.NotificarAsync(r.UsuarioId,
                $"Tu reclamo de “{objeto.Titulo}” no fue aprobado. Si crees que es un error, acércate a Seguridad con tu carné.",
                "/LostFound", "x-circle");
        }

        TempData["Toast"] = aprobar ? "Reclamo aprobado. El objeto figura como entregado." : "Reclamo rechazado.";
        return RedirectToAction(nameof(Reclamos), new { id = objeto.Id });
    }

    /// <summary>Quien reportó marca el objeto como entregado (encontrado) o recuperado (perdido).</summary>
    [HttpPost]
    public async Task<IActionResult> MarcarDevuelto(int id)
    {
        var objeto = await MiObjetoAsync(id);
        if (objeto is null)
            return NotFound();

        objeto.Estado = EstadoObjeto.Devuelto;
        await db.SaveChangesAsync();
        TempData["Toast"] = objeto.Tipo == TipoReporte.Perdido ? "¡Qué bien que lo recuperaste!" : "Marcado como entregado a su dueño.";
        return RedirectToAction(nameof(Index), new { tipo = objeto.Tipo });
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var objeto = await MiObjetoAsync(id);
        if (objeto is null)
            return NotFound();

        db.ObjetosPerdidos.Remove(objeto);
        await db.SaveChangesAsync();

        if (objeto.FotoUrl is not null)
        {
            var ruta = almacen.RutaFisica(objeto.FotoUrl);
            if (System.IO.File.Exists(ruta))
                System.IO.File.Delete(ruta);
        }

        TempData["Toast"] = "Reporte eliminado.";
        return RedirectToAction(nameof(Index), new { tipo = objeto.Tipo });
    }

    /// <summary>El objeto solo si lo reportó el usuario logueado.</summary>
    private Task<ObjetoPerdido?> MiObjetoAsync(int id)
    {
        var userId = User.GetUserId();
        return db.ObjetosPerdidos.FirstOrDefaultAsync(o => o.Id == id && o.ReportanteId == userId);
    }

    private string PrimerError() =>
        ModelState.Values.SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Revisa los datos del formulario.";
}
