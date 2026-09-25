using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Data.Seed;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

/// <summary>
/// Cátedra &amp; Tips: por cada curso, los docentes que lo dictan con la opinión de los alumnos
/// (claridad y exigencia), la dificultad comunitaria y un muro de tips con votos.
/// </summary>
public class CatedraController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? q, int? ciclo)
    {
        var cursos = await db.Cursos.AsNoTracking()
            .Include(c => c.Secciones.Where(s => s.Periodo == BaseSeeder.Periodo)).ThenInclude(s => s.Docente)
            .OrderBy(c => c.Ciclo).ThenBy(c => c.Nombre)
            .ToListAsync();

        var estadisticas = await db.Tips.AsNoTracking()
            .Where(t => t.Reportes < Tip.ReportesParaOcultar)
            .GroupBy(t => t.CursoId)
            .Select(g => new { CursoId = g.Key, Cantidad = g.Count(), Dificultad = g.Average(t => (double?)t.Dificultad) })
            .ToDictionaryAsync(x => x.CursoId);

        var lista = cursos
            .Where(c => ciclo is null || c.Ciclo == ciclo)
            .Select(c =>
            {
                estadisticas.TryGetValue(c.Id, out var e);
                var docentes = c.Secciones.Select(s => s.Docente.NombreDocente).Distinct().ToList();
                return new CursoCatedraVM(c.Id, c.Codigo, c.Nombre, c.Creditos, c.Ciclo, e?.Dificultad, e?.Cantidad ?? 0, docentes);
            })
            .Where(c => string.IsNullOrWhiteSpace(q)
                || Contiene(c.Nombre, q) || Contiene(c.Codigo, q) || c.Docentes.Any(d => Contiene(d, q)))
            .ToList();

        return View(new CatedraIndexViewModel
        {
            Cursos = lista,
            Barra = new CatedraBarraVM(q, ciclo, PuedeAportar),
            Formulario = await FormularioAsync(null)
        });
    }

    public async Task<IActionResult> Curso(int id, string orden = "utiles")
    {
        var curso = await db.Cursos.AsNoTracking()
            .Include(c => c.Secciones.Where(s => s.Periodo == BaseSeeder.Periodo)).ThenInclude(s => s.Docente)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (curso is null)
            return NotFound();

        var userId = User.GetUserId();
        var tips = await db.Tips.AsNoTracking()
            .Include(t => t.Autor)
            .Include(t => t.Docente)
            .Where(t => t.CursoId == id && t.Reportes < Tip.ReportesParaOcultar)
            .ToListAsync();
        var misVotos = await db.VotosTips.AsNoTracking()
            .Where(v => v.UsuarioId == userId && v.Tip.CursoId == id)
            .ToDictionaryAsync(v => v.TipId, v => v.Valor);
        var misReportes = (await db.ReportesTips.AsNoTracking()
            .Where(r => r.UsuarioId == userId && r.Tip.CursoId == id)
            .Select(r => r.TipId)
            .ToListAsync()).ToHashSet();

        var docentes = curso.Secciones
            .GroupBy(s => s.DocenteId)
            .Select(g =>
            {
                var docente = g.First().Docente;
                var opiniones = tips.Where(t => t.DocenteId == g.Key).ToList();
                var claridades = opiniones.Where(t => t.Claridad.HasValue).Select(t => (double)t.Claridad!.Value).ToList();
                var exigencias = opiniones.Where(t => t.Exigencia.HasValue).Select(t => (double)t.Exigencia!.Value).ToList();
                return new DocenteCatedraVM(
                    docente.NombreDocente,
                    docente.Iniciales,
                    string.Join(", ", g.Select(s => s.Codigo).Order()),
                    claridades.Count > 0 ? Math.Round(claridades.Average(), 1) : null,
                    exigencias.Count > 0 ? (Exigencia)(int)Math.Round(exigencias.Average()) : null,
                    opiniones.Count);
            })
            .OrderBy(d => d.Secciones)
            .ToList();

        var ordenados = orden == "recientes"
            ? tips.OrderByDescending(t => t.Fecha)
            : tips.OrderByDescending(t => t.Utiles - t.NoUtiles).ThenByDescending(t => t.Fecha);

        var dificultades = tips.Where(t => t.Dificultad.HasValue).Select(t => (double)t.Dificultad!.Value).ToList();

        return View(new CatedraCursoViewModel
        {
            Curso = curso,
            Dificultad = dificultades.Count > 0 ? Math.Round(dificultades.Average(), 1) : null,
            OpinionesDificultad = dificultades.Count,
            Docentes = docentes,
            Tips = ordenados.Select(t => new TipVM(
                t.Id, t.Autor.NombreCorto, t.Autor.Iniciales, t.Docente?.NombreDocente, t.Texto, t.Fecha,
                t.Utiles, t.NoUtiles, misVotos.GetValueOrDefault(t.Id), t.AutorId == userId, misReportes.Contains(t.Id))).ToList(),
            Orden = orden == "recientes" ? "recientes" : "utiles",
            Barra = new CatedraBarraVM(null, null, PuedeAportar),
            Formulario = await FormularioAsync(id)
        });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Alumno)]
    public async Task<IActionResult> Aportar(AportarTipVM vm)
    {
        if (vm.CursoId is not null && !await db.Cursos.AnyAsync(c => c.Id == vm.CursoId))
            ModelState.AddModelError(nameof(vm.CursoId), "El curso no existe.");

        var conDocente = !string.IsNullOrEmpty(vm.DocenteId);
        if (conDocente && !await db.Secciones.AnyAsync(s => s.CursoId == vm.CursoId && s.DocenteId == vm.DocenteId))
            ModelState.AddModelError(nameof(vm.DocenteId), "Ese docente no dicta el curso seleccionado.");

        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = PrimerError();
            return vm.CursoId is null ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Curso), new { id = vm.CursoId });
        }

        db.Tips.Add(new Tip
        {
            CursoId = vm.CursoId!.Value,
            DocenteId = conDocente ? vm.DocenteId : null,
            AutorId = User.GetUserId(),
            Texto = vm.Texto.Trim(),
            Dificultad = vm.Dificultad,
            // La claridad y la exigencia son del docente: sin docente no aplican.
            Claridad = conDocente ? vm.Claridad : null,
            Exigencia = conDocente ? vm.Exigencia : null
        });
        await db.SaveChangesAsync();

        TempData["Toast"] = "¡Gracias por tu tip! Ya está en el muro.";
        return RedirectToAction(nameof(Curso), new { id = vm.CursoId, orden = "recientes" });
    }

    /// <summary>
    /// Voto 👍 (valor 1) o 👎 (valor -1). Votar igual otra vez quita el voto; votar distinto lo cambia.
    /// Responde JSON para actualizar los contadores sin recargar.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Votar(int id, int valor)
    {
        if (valor is not (1 or -1))
            return BadRequest(new { error = "Voto inválido." });

        var userId = User.GetUserId();
        var tip = await db.Tips.FirstOrDefaultAsync(t => t.Id == id);
        if (tip is null)
            return NotFound();
        if (tip.AutorId == userId)
            return BadRequest(new { error = "No puedes votar tu propio tip." });

        var voto = await db.VotosTips.FirstOrDefaultAsync(v => v.TipId == id && v.UsuarioId == userId);
        if (voto is null)
        {
            voto = new VotoTip { TipId = id, UsuarioId = userId, Valor = valor };
            db.VotosTips.Add(voto);
            Contar(tip, valor, +1);
        }
        else
        {
            Contar(tip, voto.Valor, -1);
            if (voto.Valor == valor)
            {
                db.VotosTips.Remove(voto);
                voto = null;
            }
            else
            {
                voto.Valor = valor;
                Contar(tip, valor, +1);
            }
        }
        await db.SaveChangesAsync();

        return Json(new { utiles = tip.Utiles, noUtiles = tip.NoUtiles, miVoto = voto?.Valor ?? 0 });
    }

    /// <summary>Reporta un tip inapropiado. Con 3 reportes, el tip deja de mostrarse.</summary>
    [HttpPost]
    public async Task<IActionResult> Reportar(int id)
    {
        var userId = User.GetUserId();
        var tip = await db.Tips.FirstOrDefaultAsync(t => t.Id == id);
        if (tip is null)
            return NotFound();
        if (tip.AutorId == userId)
            return BadRequest(new { error = "No puedes reportar tu propio tip." });

        if (!await db.ReportesTips.AnyAsync(r => r.TipId == id && r.UsuarioId == userId))
        {
            db.ReportesTips.Add(new ReporteTip { TipId = id, UsuarioId = userId });
            tip.Reportes++;
            await db.SaveChangesAsync();
        }

        return Json(new { oculto = tip.Oculto });
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var userId = User.GetUserId();
        var tip = await db.Tips.FirstOrDefaultAsync(t => t.Id == id && t.AutorId == userId);
        if (tip is null)
            return NotFound();

        db.Tips.Remove(tip);
        await db.SaveChangesAsync();
        TempData["Toast"] = "Tip eliminado.";
        return RedirectToAction(nameof(Curso), new { id = tip.CursoId });
    }

    private bool PuedeAportar => User.IsInRole(Roles.Alumno);

    private async Task<AportarTipFormVM> FormularioAsync(int? cursoId)
    {
        var cursos = await db.Cursos.AsNoTracking()
            .OrderBy(c => c.Ciclo).ThenBy(c => c.Nombre)
            .Select(c => new OpcionCursoVM(c.Id, c.Nombre, c.Ciclo))
            .ToListAsync();

        var secciones = await db.Secciones.AsNoTracking()
            .Include(s => s.Docente)
            .Where(s => s.Periodo == BaseSeeder.Periodo)
            .ToListAsync();

        var docentes = secciones
            .GroupBy(s => (s.CursoId, s.DocenteId))
            .Select(g => new OpcionDocenteVM(g.Key.CursoId, g.Key.DocenteId, g.First().Docente.NombreDocente,
                string.Join(", ", g.Select(s => s.Codigo).Order())))
            .OrderBy(d => d.Nombre)
            .ToList();

        return new AportarTipFormVM { Cursos = cursos, Docentes = docentes, CursoSeleccionado = cursoId };
    }

    private static void Contar(Tip tip, int valor, int delta)
    {
        if (valor == 1)
            tip.Utiles += delta;
        else
            tip.NoUtiles += delta;
    }

    /// <summary>Búsqueda sin distinguir mayúsculas ni tildes ("ramirez" encuentra "Ramírez").</summary>
    private static bool Contiene(string texto, string busqueda) =>
        HoraPeru.Cultura.CompareInfo.IndexOf(texto, busqueda.Trim(), CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

    private string PrimerError() =>
        ModelState.Values.SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Revisa los datos del formulario.";
}
