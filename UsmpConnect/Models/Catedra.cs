using System.ComponentModel.DataAnnotations;

namespace UsmpConnect.Models;

/// <summary>Qué tan exigente es un docente, según los alumnos.</summary>
public enum Exigencia
{
    Baja = 1,
    Media = 2,
    Alta = 3,
    [Display(Name = "Muy Alta")] MuyAlta = 4
}

/// <summary>
/// Consejo de un alumno sobre un curso (y opcionalmente sobre un docente). Además del texto,
/// cada tip puede calificar la dificultad del curso y la claridad y exigencia del docente;
/// con esos datos se calculan los promedios comunitarios de la cátedra.
/// </summary>
public class Tip
{
    public const int ReportesParaOcultar = 3;

    public int Id { get; set; }

    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;

    /// <summary>null = tip general del curso, sin docente específico.</summary>
    public string? DocenteId { get; set; }
    public ApplicationUser? Docente { get; set; }

    public string AutorId { get; set; } = "";
    public ApplicationUser Autor { get; set; } = null!;

    [MaxLength(1000)]
    public string Texto { get; set; } = "";

    /// <summary>Dificultad del curso, de 1 (fácil) a 5 (muy difícil).</summary>
    [Range(1, 5)]
    public int? Dificultad { get; set; }

    /// <summary>Claridad del docente al explicar, de 1 a 5 estrellas.</summary>
    [Range(1, 5)]
    public int? Claridad { get; set; }

    public Exigencia? Exigencia { get; set; }

    /// <summary>Contadores de votos 👍 / 👎 (el detalle de quién votó está en <see cref="VotoTip"/>).</summary>
    public int Utiles { get; set; }
    public int NoUtiles { get; set; }

    public int Reportes { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public List<VotoTip> Votos { get; set; } = [];

    public bool Oculto => Reportes >= ReportesParaOcultar;
}

/// <summary>Voto de un usuario en un tip: +1 útil, -1 no útil. Un voto por usuario y tip.</summary>
public class VotoTip
{
    public int Id { get; set; }

    public int TipId { get; set; }
    public Tip Tip { get; set; } = null!;

    public string UsuarioId { get; set; } = "";
    public ApplicationUser Usuario { get; set; } = null!;

    public int Valor { get; set; }
}

/// <summary>Reporte de un tip inapropiado. Con <see cref="Tip.ReportesParaOcultar"/> reportes, el tip se oculta.</summary>
public class ReporteTip
{
    public int Id { get; set; }

    public int TipId { get; set; }
    public Tip Tip { get; set; } = null!;

    public string UsuarioId { get; set; } = "";
    public ApplicationUser Usuario { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}

/// <summary>Estrellas y etiquetas de Cátedra &amp; Tips para las vistas.</summary>
public static class CatedraUi
{
    /// <summary>Clases de Bootstrap Icons para 5 estrellas (llenas, media y vacías).</summary>
    public static IEnumerable<string> Estrellas(double valor)
    {
        for (var i = 1; i <= 5; i++)
            yield return valor >= i - 0.25 ? "bi-star-fill" : valor >= i - 0.75 ? "bi-star-half" : "bi-star";
    }

    public static string ClaseTag(this Exigencia e) => e switch
    {
        Exigencia.Baja => "tag-ok",
        Exigencia.Media => "tag-warn",
        Exigencia.Alta => "tag-alta",
        _ => "tag-bad"
    };
}
