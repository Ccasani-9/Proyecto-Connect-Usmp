using System.ComponentModel.DataAnnotations;

namespace UsmpConnect.Models;

public enum TipoDocumento
{
    Resumen,
    [Display(Name = "Examen resuelto")] Examen,
    [Display(Name = "Guía de laboratorio")] GuiaLab,
    [Display(Name = "Apuntes de clase")] Apuntes
}

/// <summary>Recurso del Banco de Apuntes (resumen, examen, guía...) que comparte un alumno.</summary>
public class Apunte
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Titulo { get; set; } = "";

    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;

    /// <summary>Docente del curso tal como lo escribió el alumno ("Ing. Ramírez").</summary>
    [MaxLength(60)]
    public string Profesor { get; set; } = "";

    public TipoDocumento Tipo { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Ruta del archivo subido. Es null en los apuntes de ejemplo: su PDF se genera al momento
    /// de verlo o descargarlo (así no dependen del disco, que en Render se borra).
    /// </summary>
    [MaxLength(200)]
    public string? ArchivoUrl { get; set; }

    [MaxLength(150)]
    public string NombreArchivo { get; set; } = "";

    /// <summary>".pdf", ".doc" o ".docx".</summary>
    [MaxLength(10)]
    public string Extension { get; set; } = ".pdf";

    public long Bytes { get; set; }

    public string AutorId { get; set; } = "";
    public ApplicationUser Autor { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public int Descargas { get; set; }

    /// <summary>Suma y cantidad de calificaciones (el detalle está en <see cref="CalificacionApunte"/>).</summary>
    public int SumaEstrellas { get; set; }
    public int NumCalificaciones { get; set; }

    public double Promedio => NumCalificaciones == 0 ? 0 : Math.Round((double)SumaEstrellas / NumCalificaciones, 1);

    public bool EsPdf => Extension == ".pdf";

    public string Formato => EsPdf ? "PDF" : "DOC";

    public string Tamano => Bytes switch
    {
        >= 1024 * 1024 => $"{Bytes / 1024d / 1024:0.0} MB",
        >= 1024 => $"{Bytes / 1024d:0} KB",
        _ => $"{Bytes} B"
    };

    /// <summary>PDF de un apunte de ejemplo (sin archivo subido). Requiere <see cref="Curso"/> cargado.</summary>
    public byte[] GenerarPdfEjemplo() => Services.PdfSimple.Generar(Titulo,
    [
        $"Curso: {Curso.Nombre} ({Curso.Codigo}) - {Curso.CicloRomano} Ciclo",
        $"Docente: {Profesor}",
        $"Tipo: {Services.EnumExtensions.Nombre(Tipo)}",
        "",
        Descripcion ?? "",
        "",
        "Documento de ejemplo del Banco de Apuntes de USMP Connect.",
        "Los apuntes que suben los alumnos se descargan tal como fueron subidos."
    ]);
}

/// <summary>Calificación de 1 a 5 estrellas. Una por usuario y apunte (se puede cambiar).</summary>
public class CalificacionApunte
{
    public int Id { get; set; }

    public int ApunteId { get; set; }
    public Apunte Apunte { get; set; } = null!;

    public string UsuarioId { get; set; } = "";
    public ApplicationUser Usuario { get; set; } = null!;

    [Range(1, 5)]
    public int Estrellas { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
