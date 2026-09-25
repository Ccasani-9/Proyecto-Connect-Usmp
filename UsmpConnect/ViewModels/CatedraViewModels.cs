using System.ComponentModel.DataAnnotations;
using UsmpConnect.Models;

namespace UsmpConnect.ViewModels;

public record OpcionCursoVM(int Id, string Nombre, int Ciclo);

public record OpcionDocenteVM(int CursoId, string DocenteId, string Nombre, string Secciones);

/// <summary>Opciones del modal "Aportar un Tip".</summary>
public class AportarTipFormVM
{
    public List<OpcionCursoVM> Cursos { get; set; } = [];
    public List<OpcionDocenteVM> Docentes { get; set; } = [];
    public int? CursoSeleccionado { get; set; }
}

/// <summary>Barra superior (buscador, ciclos y botón) compartida por las vistas de Cátedra.</summary>
public record CatedraBarraVM(string? Busqueda, int? Ciclo, bool PuedeAportar);

public record CursoCatedraVM(int Id, string Codigo, string Nombre, int Creditos, int Ciclo,
                             double? Dificultad, int NumTips, List<string> Docentes);

public class CatedraIndexViewModel
{
    public List<CursoCatedraVM> Cursos { get; set; } = [];
    public CatedraBarraVM Barra { get; set; } = null!;
    public AportarTipFormVM Formulario { get; set; } = null!;
}

public record DocenteCatedraVM(string Nombre, string Iniciales, string Secciones,
                               double? Claridad, Exigencia? Exigencia, int NumOpiniones);

public record TipVM(int Id, string Autor, string AutorIniciales, string? Docente, string Texto, DateTime Fecha,
                    int Utiles, int NoUtiles, int MiVoto, bool Propio, bool Reportado);

public class CatedraCursoViewModel
{
    public Curso Curso { get; set; } = null!;
    public double? Dificultad { get; set; }
    public int OpinionesDificultad { get; set; }
    public List<DocenteCatedraVM> Docentes { get; set; } = [];
    public List<TipVM> Tips { get; set; } = [];

    /// <summary>"utiles" o "recientes".</summary>
    public string Orden { get; set; } = "utiles";

    public CatedraBarraVM Barra { get; set; } = null!;
    public AportarTipFormVM Formulario { get; set; } = null!;
}

public class AportarTipVM
{
    [Required(ErrorMessage = "Selecciona el curso.")]
    public int? CursoId { get; set; }

    /// <summary>Vacío = tip general del curso.</summary>
    public string? DocenteId { get; set; }

    [Required(ErrorMessage = "Escribe tu tip.")]
    [StringLength(1000, MinimumLength = 20, ErrorMessage = "El tip debe tener entre 20 y 1000 caracteres.")]
    public string Texto { get; set; } = "";

    [Range(1, 5, ErrorMessage = "La dificultad va de 1 a 5.")]
    public int? Dificultad { get; set; }

    [Range(1, 5, ErrorMessage = "La claridad va de 1 a 5.")]
    public int? Claridad { get; set; }

    public Exigencia? Exigencia { get; set; }
}
