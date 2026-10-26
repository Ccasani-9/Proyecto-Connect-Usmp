using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using UsmpConnect.Data;
using UsmpConnect.Models;

namespace UsmpConnect.Services.Agente;

/// <summary>Proyecciones explícitas de contenido público: no expone perfiles ni datos de contacto al modelo.</summary>
public sealed class CatalogoFiaPlugin(ApplicationDbContext db, ContextoAsistente contexto)
{
    private static string Consulta(string texto) => (texto ?? "").Trim()[..Math.Min((texto ?? "").Trim().Length, 100)];

    [KernelFunction, Description("Busca hasta cuatro restaurantes cercanos ordenados por valoración. Usa texto vacío para recomendaciones generales. precioMaximo filtra locales que tienen algún plato dentro del presupuesto; no todos sus platos cuestan ese importe.")]
    public async Task<object> BuscarRestaurantes(string texto = "", decimal? precioMaximo = null, CancellationToken cancellationToken = default)
    {
        texto = Consulta(texto);
        var query = db.Restaurantes.AsNoTracking();
        if (precioMaximo.HasValue) query = query.Where(r => r.Platos.Any(p => p.Precio <= precioMaximo.Value));
        var candidatos = await query.Select(r => new {
                r.Id, r.Nombre, r.Descripcion, r.Direccion, r.DistanciaTexto, r.RangoPrecios, r.CalificacionPromedio, r.TotalResenas,
                Platos = r.Platos.Where(p => !precioMaximo.HasValue || p.Precio <= precioMaximo.Value)
                    .OrderBy(p => p.Precio).Select(p => new { p.Nombre, p.Precio }).ToList()
            }).ToListAsync(cancellationToken);
        var locales = candidatos.Where(r => texto == "" || BuscadorAlgolia.Contiene(r.Nombre, texto)
                || BuscadorAlgolia.Contiene(r.Descripcion, texto) || r.Platos.Any(p => BuscadorAlgolia.Contiene(p.Nombre, texto)))
            .OrderByDescending(r => r.CalificacionPromedio).ThenByDescending(r => r.TotalResenas).Take(4)
            .Select(r => new { r.Id, r.Nombre, r.Descripcion, r.Direccion, r.DistanciaTexto, r.RangoPrecios,
                r.CalificacionPromedio, r.TotalResenas,
                Platos = r.Platos.OrderByDescending(p => texto != "" && BuscadorAlgolia.Contiene(p.Nombre, texto)).Take(3).ToList() }).ToList();
        foreach (var r in locales) contexto.Agregar(new(r.Nombre,
            $"{r.RangoPrecios} · {r.CalificacionPromedio:0.0}/5 ({r.TotalResenas} reseñas)",
            $"/Restaurantes?q={Uri.EscapeDataString(r.Nombre)}#card-rest-{r.Id}", "Restaurantes"));
        return locales;
    }

    [KernelFunction, Description("Busca cursos por nombre, código o apellido de docente y devuelve consejos visibles aportados por alumnos. Son opiniones, no afirmaciones oficiales.")]
    public async Task<object> BuscarCursosYCatedra(string texto, CancellationToken cancellationToken = default)
    {
        texto = Consulta(texto);
        if (texto.Length < 2) return Array.Empty<object>();
        var candidatos = await db.Cursos.AsNoTracking().Select(c => new { c.Id, c.Nombre, c.Codigo, c.Ciclo,
            Docentes = c.Secciones.Select(s => s.Docente.Nombres + " " + s.Docente.Apellidos).ToList() }).ToListAsync(cancellationToken);
        var cursos = candidatos.Where(c => BuscadorAlgolia.Contiene(c.Nombre, texto) || BuscadorAlgolia.Contiene(c.Codigo, texto)
            || c.Docentes.Any(d => BuscadorAlgolia.Contiene(d, texto))).OrderBy(c => c.Nombre).Take(4).ToList();
        var ids = cursos.Select(c => c.Id).ToArray();
        var tips = await db.Tips.AsNoTracking().Where(t => ids.Contains(t.CursoId) && t.Reportes < Tip.ReportesParaOcultar)
            .OrderByDescending(t => t.Utiles).Take(8).Select(t => new { t.CursoId, t.Texto, t.Dificultad, t.Utiles }).ToListAsync(cancellationToken);
        foreach (var c in cursos) contexto.Agregar(new(c.Nombre, $"{c.Codigo} · Ciclo {c.Ciclo}", $"/Catedra/Curso/{c.Id}", "Cátedra"));
        return new { cursos, consejosDeAlumnos = tips };
    }

    [KernelFunction, Description("Busca hasta cuatro apuntes publicados por título, descripción o curso. Devuelve metadatos y enlaces; no interpreta el contenido del archivo subido.")]
    public async Task<object> BuscarApuntes(string texto, CancellationToken cancellationToken = default)
    {
        texto = Consulta(texto);
        if (texto.Length < 2) return Array.Empty<object>();
        var candidatos = await db.Apuntes.AsNoTracking().OrderByDescending(a => a.Fecha)
            .Select(a => new { a.Id, a.Titulo, Curso = a.Curso.Nombre, a.Descripcion }).ToListAsync(cancellationToken);
        var apuntes = candidatos.Where(a => BuscadorAlgolia.Contiene(a.Titulo, texto) || BuscadorAlgolia.Contiene(a.Curso, texto)
            || BuscadorAlgolia.Contiene(a.Descripcion ?? "", texto)).Take(4).ToList();
        foreach (var a in apuntes) contexto.Agregar(new(a.Titulo, a.Curso, $"/Apuntes/Ver/{a.Id}", "Apuntes"));
        return apuntes;
    }

    [KernelFunction, Description("Busca artículos disponibles del Marketplace por título. No devuelve teléfonos ni información privada del vendedor.")]
    public async Task<object> BuscarMarketplace(string texto, decimal? precioMaximo = null, CancellationToken cancellationToken = default)
    {
        texto = Consulta(texto);
        if (texto.Length < 2) return Array.Empty<object>();
        var candidatos = await db.Articulos.AsNoTracking().Where(a => a.Estado == EstadoPublicacion.Disponible
                && (!precioMaximo.HasValue || !a.Precio.HasValue || a.Precio <= precioMaximo.Value))
            .OrderByDescending(a => a.FechaPublicacion)
            .Select(a => new { a.Id, a.Titulo, a.Precio, a.EstadoItem }).ToListAsync(cancellationToken);
        var items = candidatos.Where(a => BuscadorAlgolia.Contiene(a.Titulo, texto)).Take(4).ToList();
        foreach (var a in items) contexto.Agregar(new(a.Titulo, a.Precio is > 0 ? $"S/ {a.Precio:0.00}" : "Gratis",
            $"/Marketplace?q={Uri.EscapeDataString(a.Titulo)}", "Marketplace"));
        return items;
    }
}
