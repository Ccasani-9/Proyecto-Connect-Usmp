using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using UsmpConnect.Data;
using UsmpConnect.Models;

namespace UsmpConnect.Services.Agente;

public record ApunteAsistente(int Id, string Titulo, string Curso, string? Descripcion);
public record BusquedaApuntes(IReadOnlyList<ApunteAsistente> Resultados, IReadOnlyList<ApunteAsistente> OtrosMaterialesDisponibles);

/// <summary>Proyecciones explícitas de contenido público: no expone perfiles ni datos de contacto al modelo.</summary>
public sealed class CatalogoFiaPlugin(ApplicationDbContext db, ContextoAsistente contexto, ConsultaAsistente? solicitud = null)
{
    private static string Consulta(string texto) => (texto ?? "").Trim()[..Math.Min((texto ?? "").Trim().Length, 100)];

    [KernelFunction, Description("Busca restaurantes por categoría, nombre o tipo de comida. Usa categoria='ComidaRapida' para comida rápida y texto vacío si no piden un plato/local concreto. precioMaximo filtra el gasto PROMEDIO REFERENCIAL por persona del local, NO precios de platos. Los importes son estimaciones demo, no precios garantizados. Devuelve resultados y, solo si no hay coincidencias, alternativas con precio desconocido claramente separadas.")]
    public async Task<object> BuscarRestaurantes(string texto = "", decimal? precioMaximo = null,
        string? categoria = null, CancellationToken cancellationToken = default)
    {
        texto = Consulta(texto);
        precioMaximo = solicitud?.Presupuesto ?? precioMaximo;
        categoria = solicitud?.Categoria ?? categoria;
        var categoriaBuscada = CategoriaDesdeTexto(categoria ?? texto);
        if (!string.IsNullOrWhiteSpace(categoria) && categoriaBuscada is null)
            return new { error = "Categoría desconocida. Usa MenuCriollo, ComidaRapida, Chifa, CafeteriaHuarique, PasteleriaPostres o Saludable." };
        if (categoriaBuscada.HasValue && new[] { categoriaBuscada.Value.ToString(), categoriaBuscada.Value.Nombre(),
            "comida rapida", "criollo", "cafeteria", "postres", "saludable" }
            .Any(alias => texto.Length == alias.Length && BuscadorAlgolia.Contiene(texto, alias))) texto = "";
        if (precioMaximo is <= 0) return new { error = "El presupuesto debe ser mayor que cero." };
        var query = db.Restaurantes.AsNoTracking();
        if (categoriaBuscada.HasValue) query = query.Where(r => r.Categoria == categoriaBuscada.Value);
        var candidatos = await query.Select(r => new {
            r.Id, r.Nombre, r.Descripcion, r.Direccion, r.DistanciaTexto, r.HorarioAtencion,
            Categoria = r.Categoria.ToString(), r.RangoPrecios, r.PrecioPromedioReferencial,
            r.CalificacionPromedio, r.TotalResenas,
            Platos = r.Platos.Select(p => new { p.Nombre, p.Precio }).ToList()
        }).ToListAsync(cancellationToken);
        var coincidentes = candidatos.Where(r => Coincide(texto, new[] { r.Nombre, r.Descripcion }.Concat(r.Platos.Select(p => p.Nombre)).ToArray()))
            .OrderByDescending(r => r.CalificacionPromedio).ThenByDescending(r => r.TotalResenas).ToList();
        if (solicitud?.ParaAlmorzar == true)
            coincidentes = coincidentes.Where(r => HorarioRestaurante.IncluyeAlmuerzo(r.HorarioAtencion, solicitud.DiaConsulta) != false).ToList();
        var resultados = coincidentes.Where(r => !precioMaximo.HasValue ||
            (r.PrecioPromedioReferencial.HasValue && r.PrecioPromedioReferencial <= precioMaximo.Value)).Take(4).ToList();
        var sinPrecio = coincidentes.Where(r => precioMaximo.HasValue && r.PrecioPromedioReferencial is null)
            .Take(resultados.Count == 0 ? 3 : 0).ToList();
        foreach (var r in resultados.Concat(sinPrecio))
        {
            var importe = r.PrecioPromedioReferencial.HasValue
                ? $"S/ {r.PrecioPromedioReferencial:0.00} aprox. por persona · estimación demo"
                : "Gasto promedio sin registrar; presupuesto por confirmar";
            contexto.Agregar(new(r.Nombre, $"{importe} · {r.CalificacionPromedio:0.0}/5",
                $"/Restaurantes?q={Uri.EscapeDataString(r.Nombre)}#card-rest-{r.Id}", "Restaurantes"));
        }
        return new { resultados, alternativasSinPrecioConfirmado = sinPrecio,
            presupuesto = precioMaximo, categoria = categoriaBuscada?.ToString(),
            aviso = "Gasto promedio referencial por persona, estimado para la maqueta. No garantiza un plato, precio real ni que esté abierto ahora; consulta horario y confirma con el local.",
            menorPrecioReferencial = coincidentes.Where(r => r.PrecioPromedioReferencial.HasValue)
                .Select(r => r.PrecioPromedioReferencial).DefaultIfEmpty().Min() };
    }

    private static CategoriaRestaurante? CategoriaDesdeTexto(string? texto)
    {
        texto = (texto ?? "").Trim();
        foreach (var categoria in Enum.GetValues<CategoriaRestaurante>())
            if (BuscadorAlgolia.Contiene(texto, categoria.ToString()) || BuscadorAlgolia.Contiene(texto, categoria.Nombre()))
                return categoria;
        if (BuscadorAlgolia.Contiene(texto, "comida rapida")) return CategoriaRestaurante.ComidaRapida;
        if (BuscadorAlgolia.Contiene(texto, "criollo")) return CategoriaRestaurante.MenuCriollo;
        if (BuscadorAlgolia.Contiene(texto, "cafeteria")) return CategoriaRestaurante.CafeteriaHuarique;
        if (BuscadorAlgolia.Contiene(texto, "postres")) return CategoriaRestaurante.PasteleriaPostres;
        if (BuscadorAlgolia.Contiene(texto, "saludable")) return CategoriaRestaurante.Saludable;
        return null;
    }

    private static bool Coincide(string texto, params string[] campos)
    {
        var terminos = System.Text.RegularExpressions.Regex.Matches(texto, @"\p{L}+|\d+")
            .Select(m => m.Value).Where(t => t.Length > 1 &&
                !new[] { "de", "del", "la", "el", "los", "las", "en", "para", "con", "apuntes", "resumen" }.Contains(t.ToLowerInvariant())).ToArray();
        return terminos.All(t => campos.Any(c => BuscadorAlgolia.Contiene(c, t)));
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
        if (texto.Length < 2) return new BusquedaApuntes([], []);
        var candidatos = await db.Apuntes.AsNoTracking().OrderByDescending(a => a.Fecha)
            .Select(a => new ApunteAsistente(a.Id, a.Titulo, a.Curso.Nombre, a.Descripcion)).ToListAsync(cancellationToken);
        var principales = candidatos.Where(a => Coincide(texto, a.Titulo, a.Curso)).ToList();
        var apuntes = (principales.Count > 0 ? principales : candidatos.Where(a => Coincide(texto, a.Titulo, a.Curso, a.Descripcion ?? "")))
            .Take(4).ToList();
        foreach (var a in apuntes) contexto.Agregar(new(a.Titulo, a.Curso, $"/Apuntes/Ver/{a.Id}", "Apuntes"));
        return new BusquedaApuntes(apuntes, apuntes.Count == 0 ? candidatos.Take(2).ToList() : []);
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
