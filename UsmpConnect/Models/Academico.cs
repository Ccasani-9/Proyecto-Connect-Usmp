using System.ComponentModel.DataAnnotations;

namespace UsmpConnect.Models;

public class Curso
{
    public int Id { get; set; }

    [MaxLength(10)]
    public string Codigo { get; set; } = "";

    [MaxLength(120)]
    public string Nombre { get; set; } = "";

    /// <summary>Nombre abreviado para calendarios y etiquetas ("Cálculo Dif.").</summary>
    [MaxLength(40)]
    public string NombreCorto { get; set; } = "";

    public int Creditos { get; set; }

    /// <summary>Ciclo de la malla curricular (1 = I Ciclo, 2 = II Ciclo, ...).</summary>
    public int Ciclo { get; set; }

    public List<Seccion> Secciones { get; set; } = [];

    public string CicloRomano => Romano(Ciclo);

    public static string Romano(int n) => n switch
    {
        1 => "I", 2 => "II", 3 => "III", 4 => "IV", 5 => "V",
        6 => "VI", 7 => "VII", 8 => "VIII", 9 => "IX", 10 => "X",
        _ => n.ToString()
    };
}

/// <summary>Una sección de un curso en un periodo, dictada por un docente.</summary>
public class Seccion
{
    public int Id { get; set; }

    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;

    [MaxLength(10)]
    public string Codigo { get; set; } = "";

    [MaxLength(10)]
    public string Periodo { get; set; } = "";

    public string DocenteId { get; set; } = "";
    public ApplicationUser Docente { get; set; } = null!;

    public List<Horario> Horarios { get; set; } = [];
    public List<Matricula> Matriculas { get; set; } = [];
}

public class Horario
{
    public int Id { get; set; }

    public int SeccionId { get; set; }
    public Seccion Seccion { get; set; } = null!;

    public DayOfWeek Dia { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }

    [MaxLength(30)]
    public string Aula { get; set; } = "";

    public string Rango => $"{HoraInicio:HH\\:mm} - {HoraFin:HH\\:mm}";
}

public class Matricula
{
    public int Id { get; set; }

    public string AlumnoId { get; set; } = "";
    public ApplicationUser Alumno { get; set; } = null!;

    public int SeccionId { get; set; }
    public Seccion Seccion { get; set; } = null!;

    public Nota? Nota { get; set; }
}

/// <summary>Notas de un alumno en una sección. EP = Examen Parcial, EF = Examen Final.</summary>
public class Nota
{
    public int Id { get; set; }

    public int MatriculaId { get; set; }
    public Matricula Matricula { get; set; } = null!;

    [Range(0, 20)]
    public decimal? PP { get; set; }

    [Range(0, 20)]
    public decimal? EP { get; set; }

    [Range(0, 20)]
    public decimal? EF { get; set; }

    public DateTime? ActualizadoEn { get; set; }

    public decimal? Promedio => (PP.HasValue && EP.HasValue && EF.HasValue)
        ? Math.Round((PP.Value * 0.3m) + (EP.Value * 0.3m) + (EF.Value * 0.4m), 1)
        : (EP.HasValue && EF.HasValue)
            ? Math.Round((EP.Value + EF.Value) / 2m, 1)
            : null;
}
