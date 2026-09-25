using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Data.Seed;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

/// <summary>Banco de Apuntes: resúmenes, exámenes resueltos y guías que comparten los alumnos.</summary>
public class ApuntesController(ApplicationDbContext db, IAlmacenArchivos almacen, INotificacionService notificaciones) : Controller
{
    private const long MaxArchivo = 25 * 1024 * 1024;

    public async Task<IActionResult> Index(string? q, int? ciclo, int? cursoId, TipoDocumento? tipo, string orden = "calificacion")
    {
        var userId = User.GetUserId();
        var query = db.Apuntes.AsNoTracking().Include(a => a.Curso).Include(a => a.Autor).AsQueryable();

        if (ciclo is not null)
            query = query.Where(a => a.Curso.Ciclo == ciclo);
        if (cursoId is not null)
            query = query.Where(a => a.CursoId == cursoId);
        if (tipo is not null)
            query = query.Where(a => a.Tipo == tipo);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var texto = q.Trim().ToLower();
            query = query.Where(a => a.Titulo.ToLower().Contains(texto)
                || a.Profesor.ToLower().Contains(texto)
                || a.Curso.Nombre.ToLower().Contains(texto)
                || (a.Descripcion != null && a.Descripcion.ToLower().Contains(texto)));
        }

        var apuntes = await query.ToListAsync();
        apuntes = orden switch
        {
            "recientes" => apuntes.OrderByDescending(a => a.Fecha).ToList(),
            "descargas" => apuntes.OrderByDescending(a => a.Descargas).ToList(),
            _ => apuntes.OrderByDescending(a => a.Promedio).ThenByDescending(a => a.NumCalificaciones).ToList()
        };

        var profesores = (await db.Secciones.AsNoTracking()
                .Where(s => s.Periodo == BaseSeeder.Periodo)
                .Select(s => s.Docente)
                .Distinct()
                .ToListAsync())
            .Select(d => d.NombreDocente)
            .Order()
            .ToList();

        return View(new ApuntesIndexViewModel
        {
            Apuntes = apuntes,
            Busqueda = q,
            Ciclo = ciclo,
            CursoId = cursoId,
            Tipo = tipo,
            Orden = orden is "recientes" or "descargas" ? orden : "calificacion",
            Cursos = await db.Cursos.AsNoTracking()
                .OrderBy(c => c.Ciclo).ThenBy(c => c.Nombre)
                .Select(c => new OpcionCursoVM(c.Id, c.Nombre, c.Ciclo))
                .ToListAsync(),
            Profesores = profesores,
            MisCalificaciones = await db.CalificacionesApuntes.AsNoTracking()
                .Where(c => c.UsuarioId == userId)
                .ToDictionaryAsync(c => c.ApunteId, c => c.Estrellas),
            UsuarioId = userId
        });
    }

    [HttpPost]
    public async Task<IActionResult> Subir(SubirApunteVM vm)
    {
        if (vm.CursoId is not null && !await db.Cursos.AnyAsync(c => c.Id == vm.CursoId))
            ModelState.AddModelError(nameof(vm.CursoId), "La asignatura no existe.");

        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = PrimerError();
            return RedirectToAction(nameof(Index));
        }

        ArchivoGuardado archivo;
        try
        {
            archivo = await almacen.GuardarAsync(vm.Archivo!, "apuntes", AlmacenArchivosLocal.Documentos, MaxArchivo);
        }
        catch (ArchivoInvalidoException ex)
        {
            TempData["ToastError"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }

        db.Apuntes.Add(new Apunte
        {
            Titulo = vm.Titulo.Trim(),
            CursoId = vm.CursoId!.Value,
            Profesor = vm.Profesor.Trim(),
            Tipo = vm.Tipo,
            Descripcion = string.IsNullOrWhiteSpace(vm.Descripcion) ? null : vm.Descripcion.Trim(),
            ArchivoUrl = archivo.Url,
            NombreArchivo = archivo.NombreOriginal,
            Extension = archivo.Extension,
            Bytes = archivo.Bytes,
            AutorId = User.GetUserId()
        });
        await db.SaveChangesAsync();

        TempData["Toast"] = "¡Gracias por compartir! Tu recurso ya está en el Banco de Apuntes.";
        return RedirectToAction(nameof(Index), new { orden = "recientes" });
    }

    /// <summary>Vista previa: muestra el PDF dentro del navegador. Los Word se descargan.</summary>
    public async Task<IActionResult> Ver(int id)
    {
        var apunte = await db.Apuntes.AsNoTracking().Include(a => a.Curso).FirstOrDefaultAsync(a => a.Id == id);
        if (apunte is null)
            return NotFound();
        return apunte.EsPdf ? Archivo(apunte, descargar: false) : RedirectToAction(nameof(Descargar), new { id });
    }

    public async Task<IActionResult> Descargar(int id)
    {
        var apunte = await db.Apuntes.AsNoTracking().Include(a => a.Curso).FirstOrDefaultAsync(a => a.Id == id);
        if (apunte is null)
            return NotFound();

        await db.Apuntes.Where(a => a.Id == id).ExecuteUpdateAsync(s => s.SetProperty(a => a.Descargas, a => a.Descargas + 1));
        return Archivo(apunte, descargar: true);
    }

    /// <summary>Califica un apunte de 1 a 5 estrellas. Si ya lo calificaste, se actualiza tu calificación.</summary>
    [HttpPost]
    public async Task<IActionResult> Calificar(int id, int estrellas, string? returnUrl)
    {
        var volver = LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Apuntes");
        var userId = User.GetUserId();
        var apunte = await db.Apuntes.FirstOrDefaultAsync(a => a.Id == id);
        if (apunte is null)
            return NotFound();

        if (estrellas is < 1 or > 5)
        {
            TempData["ToastError"] = "Elige de 1 a 5 estrellas.";
            return volver;
        }
        if (apunte.AutorId == userId)
        {
            TempData["ToastError"] = "No puedes calificar tu propio apunte.";
            return volver;
        }

        var calificacion = await db.CalificacionesApuntes.FirstOrDefaultAsync(c => c.ApunteId == id && c.UsuarioId == userId);
        var nueva = calificacion is null;
        if (nueva)
        {
            db.CalificacionesApuntes.Add(new CalificacionApunte { ApunteId = id, UsuarioId = userId, Estrellas = estrellas });
            apunte.NumCalificaciones++;
            apunte.SumaEstrellas += estrellas;
        }
        else
        {
            apunte.SumaEstrellas += estrellas - calificacion!.Estrellas;
            calificacion.Estrellas = estrellas;
            calificacion.Fecha = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();

        if (nueva)
        {
            await notificaciones.NotificarAsync(apunte.AutorId,
                $"{User.GetNombre()} calificó tu apunte “{apunte.Titulo}” con {estrellas} ★.", "/Apuntes", "star");
        }

        TempData["Toast"] = nueva ? "¡Gracias por calificar!" : "Actualizamos tu calificación.";
        return volver;
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var userId = User.GetUserId();
        var apunte = await db.Apuntes.FirstOrDefaultAsync(a => a.Id == id && a.AutorId == userId);
        if (apunte is null)
            return NotFound();

        db.Apuntes.Remove(apunte);
        await db.SaveChangesAsync();

        if (apunte.ArchivoUrl is not null)
        {
            var ruta = almacen.RutaFisica(apunte.ArchivoUrl);
            if (System.IO.File.Exists(ruta))
                System.IO.File.Delete(ruta);
        }

        TempData["Toast"] = "Apunte eliminado.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Devuelve el archivo subido o, en los apuntes de ejemplo, el PDF generado.</summary>
    private IActionResult Archivo(Apunte apunte, bool descargar)
    {
        var tipoMime = apunte.Extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            _ => "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };
        // Sin nombre de archivo el navegador lo abre (vista previa); con nombre, lo descarga.
        var nombre = descargar ? apunte.NombreArchivo : null;

        if (apunte.ArchivoUrl is null)
            return File(apunte.GenerarPdfEjemplo(), tipoMime, nombre);

        var ruta = almacen.RutaFisica(apunte.ArchivoUrl);
        if (!System.IO.File.Exists(ruta))
            return NotFound();
        return PhysicalFile(ruta, tipoMime, nombre);
    }

    private string PrimerError() =>
        ModelState.Values.SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Revisa los datos del formulario.";
}
