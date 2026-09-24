using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

public class NotasController(ApplicationDbContext db) : Controller
{
    /// <summary>Alumno: todas sus notas del periodo. Docente: redirige a Inicio (sus secciones).</summary>
    public async Task<IActionResult> Index()
    {
        if (User.EsProfesor())
            return RedirectToAction("Index", "Home");

        return View(await NotasAlumnoAsync(db, User.GetUserId()));
    }

    internal static async Task<List<NotaCursoVM>> NotasAlumnoAsync(ApplicationDbContext db, string alumnoId)
    {
        var datos = await db.Matriculas.AsNoTracking()
            .Where(m => m.AlumnoId == alumnoId)
            .Select(m => new
            {
                Curso = m.Seccion.Curso.Nombre,
                m.Seccion.Curso.Creditos,
                m.Seccion.Docente.Titulo,
                m.Seccion.Docente.Apellidos,
                EP = m.Nota != null ? m.Nota.EP : null,
                EF = m.Nota != null ? m.Nota.EF : null
            })
            .ToListAsync();

        return datos
            .Select(d => new NotaCursoVM(d.Curso, $"{d.Titulo} {d.Apellidos.Split(' ')[0]}", d.Creditos, d.EP, d.EF))
            .OrderByDescending(n => n.EP.HasValue).ThenBy(n => n.Curso)
            .ToList();
    }

    /// <summary>Docente: registrar EP y EF de los alumnos de una de sus secciones.</summary>
    [Authorize(Roles = Roles.Profesor)]
    public async Task<IActionResult> Seccion(int id)
    {
        var vm = await CargarSeccionAsync(id);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Profesor)]
    public async Task<IActionResult> Seccion(int id, List<AlumnoNotaVM> alumnos)
    {
        var docenteId = User.GetUserId();
        var seccion = await db.Secciones
            .Include(s => s.Curso)
            .Include(s => s.Matriculas).ThenInclude(m => m.Nota)
            .FirstOrDefaultAsync(s => s.Id == id && s.DocenteId == docenteId);
        if (seccion is null)
            return NotFound();

        if (alumnos.Any(a => a.EP is < 0 or > 20 || a.EF is < 0 or > 20))
        {
            TempData["ToastError"] = "Las notas deben estar entre 0 y 20.";
            return RedirectToAction(nameof(Seccion), new { id });
        }

        var cambios = 0;
        foreach (var dato in alumnos)
        {
            var matricula = seccion.Matriculas.FirstOrDefault(m => m.Id == dato.MatriculaId);
            if (matricula is null)
                continue;

            matricula.Nota ??= new Nota();
            if (matricula.Nota.EP == dato.EP && matricula.Nota.EF == dato.EF)
                continue;

            matricula.Nota.EP = dato.EP;
            matricula.Nota.EF = dato.EF;
            matricula.Nota.ActualizadoEn = DateTime.UtcNow;
            cambios++;

            db.Notificaciones.Add(new Notificacion
            {
                UsuarioId = matricula.AlumnoId,
                Icono = "journal-check",
                Url = "/Notas",
                Mensaje = $"{User.GetNombre()} actualizó tus notas de {seccion.Curso.Nombre}."
            });
        }

        await db.SaveChangesAsync();
        TempData["Toast"] = cambios == 0 ? "No hubo cambios." : $"Notas guardadas: {cambios} alumno(s) notificados.";
        return RedirectToAction(nameof(Seccion), new { id });
    }

    private async Task<NotasSeccionViewModel?> CargarSeccionAsync(int id)
    {
        var docenteId = User.GetUserId();
        return await db.Secciones.AsNoTracking()
            .Where(s => s.Id == id && s.DocenteId == docenteId)
            .Select(s => new NotasSeccionViewModel
            {
                SeccionId = s.Id,
                Curso = s.Curso.Nombre,
                CodigoCurso = s.Curso.Codigo,
                Seccion = s.Codigo,
                Alumnos = s.Matriculas
                    .OrderBy(m => m.Alumno.Apellidos)
                    .Select(m => new AlumnoNotaVM
                    {
                        MatriculaId = m.Id,
                        Codigo = m.Alumno.CodigoAlumno ?? "",
                        Nombre = m.Alumno.Apellidos + ", " + m.Alumno.Nombres,
                        EP = m.Nota != null ? m.Nota.EP : null,
                        EF = m.Nota != null ? m.Nota.EF : null
                    }).ToList()
            })
            .FirstOrDefaultAsync();
    }
}
