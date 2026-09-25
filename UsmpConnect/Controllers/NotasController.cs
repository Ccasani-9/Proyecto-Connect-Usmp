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
}
