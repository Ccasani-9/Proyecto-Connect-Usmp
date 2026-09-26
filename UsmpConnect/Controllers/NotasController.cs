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
    /// <summary>Alumno: todas sus notas del periodo.</summary>
    public async Task<IActionResult> Index()
    {
        return View(await NotasAlumnoAsync(db, User.GetUserId()));
    }

    [HttpPost]
    public async Task<IActionResult> ActualizarNota(int matriculaId, decimal? pp, decimal? ep, decimal? ef)
    {
        var userId = User.GetUserId();
        var matricula = await db.Matriculas
            .Include(m => m.Nota)
            .FirstOrDefaultAsync(m => m.Id == matriculaId && m.AlumnoId == userId);

        if (matricula is null)
            return NotFound();

        if (pp is < 0 or > 20 || ep is < 0 or > 20 || ef is < 0 or > 20)
        {
            TempData["ToastError"] = "Las notas deben estar dentro del rango 0 a 20.";
            return RedirectToAction(nameof(Index));
        }

        if (matricula.Nota is null)
        {
            matricula.Nota = new Nota
            {
                MatriculaId = matricula.Id,
                PP = pp,
                EP = ep,
                EF = ef,
                ActualizadoEn = DateTime.UtcNow
            };
            db.Notas.Add(matricula.Nota);
        }
        else
        {
            matricula.Nota.PP = pp;
            matricula.Nota.EP = ep;
            matricula.Nota.EF = ef;
            matricula.Nota.ActualizadoEn = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        TempData["Toast"] = "Notas actualizadas exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    internal static async Task<List<NotaCursoVM>> NotasAlumnoAsync(ApplicationDbContext db, string alumnoId)
    {
        var datos = await db.Matriculas.AsNoTracking()
            .Where(m => m.AlumnoId == alumnoId)
            .Select(m => new
            {
                MatriculaId = m.Id,
                Curso = m.Seccion.Curso.Nombre,
                m.Seccion.Curso.Creditos,
                m.Seccion.Docente.Titulo,
                m.Seccion.Docente.Apellidos,
                PP = m.Nota != null ? m.Nota.PP : null,
                EP = m.Nota != null ? m.Nota.EP : null,
                EF = m.Nota != null ? m.Nota.EF : null
            })
            .ToListAsync();

        return datos
            .Select(d => new NotaCursoVM(d.MatriculaId, d.Curso, $"{d.Titulo} {d.Apellidos.Split(' ')[0]}", d.Creditos, d.PP, d.EP, d.EF))
            .OrderByDescending(n => n.EP.HasValue || n.PP.HasValue).ThenBy(n => n.Curso)
            .ToList();
    }
}
