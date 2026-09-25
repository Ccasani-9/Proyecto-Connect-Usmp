using System.ComponentModel.DataAnnotations;
using UsmpConnect.Models;

namespace UsmpConnect.ViewModels;

public class ApuntesIndexViewModel
{
    public List<Apunte> Apuntes { get; set; } = [];

    // Filtros actuales
    public string? Busqueda { get; set; }
    public int? Ciclo { get; set; }
    public int? CursoId { get; set; }
    public TipoDocumento? Tipo { get; set; }

    /// <summary>"calificacion", "recientes" o "descargas".</summary>
    public string Orden { get; set; } = "calificacion";

    public List<OpcionCursoVM> Cursos { get; set; } = [];
    public List<string> Profesores { get; set; } = [];

    /// <summary>Estrellas que el usuario ya dio, por id de apunte.</summary>
    public Dictionary<int, int> MisCalificaciones { get; set; } = [];

    public string UsuarioId { get; set; } = "";
}

public class SubirApunteVM
{
    [Required(ErrorMessage = "Escribe el título del recurso.")]
    [StringLength(120, ErrorMessage = "El título puede tener hasta 120 caracteres.")]
    public string Titulo { get; set; } = "";

    [Required(ErrorMessage = "Selecciona la asignatura.")]
    public int? CursoId { get; set; }

    [Required(ErrorMessage = "Escribe el nombre del profesor.")]
    [StringLength(60, ErrorMessage = "El nombre del profesor puede tener hasta 60 caracteres.")]
    public string Profesor { get; set; } = "";

    public TipoDocumento Tipo { get; set; }

    [StringLength(500, ErrorMessage = "La descripción puede tener hasta 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "Adjunta el archivo del recurso.")]
    public IFormFile? Archivo { get; set; }
}
