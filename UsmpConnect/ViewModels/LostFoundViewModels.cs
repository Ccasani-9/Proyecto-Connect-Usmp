using System.ComponentModel.DataAnnotations;
using UsmpConnect.Models;

namespace UsmpConnect.ViewModels;

public class LostFoundIndexViewModel
{
    public TipoReporte Tipo { get; set; }
    public List<ObjetoPerdido> Objetos { get; set; } = [];
    public string UsuarioId { get; set; } = "";

    /// <summary>Para autocompletar el formulario de reclamo.</summary>
    public string MiCodigo { get; set; } = "";
    public string MiNombre { get; set; } = "";

    /// <summary>Objetos que el usuario ya reclamó y siguen pendientes de revisión.</summary>
    public HashSet<int> MisReclamos { get; set; } = [];
}

public class ReportarObjetoVM
{
    public TipoReporte Tipo { get; set; }

    [Required(ErrorMessage = "Escribe qué objeto es.")]
    [StringLength(100, ErrorMessage = "El título puede tener hasta 100 caracteres.")]
    public string Titulo { get; set; } = "";

    [Required(ErrorMessage = "Selecciona una categoría.")]
    public CategoriaObjeto? Categoria { get; set; }

    [Required(ErrorMessage = "Selecciona el pabellón o edificio.")]
    public string Pabellon { get; set; } = "";

    [StringLength(100, ErrorMessage = "El lugar exacto puede tener hasta 100 caracteres.")]
    public string? Lugar { get; set; }

    [Required(ErrorMessage = "Indica la fecha y hora.")]
    public DateTime? FechaHallazgo { get; set; }

    public IFormFile? Foto { get; set; }

    public CustodiaObjeto? Custodia { get; set; }

    [RegularExpression(Services.WhatsApp.Patron, ErrorMessage = "El WhatsApp debe tener 9 dígitos y empezar con 9.")]
    public string? WhatsApp { get; set; }

    [StringLength(300, ErrorMessage = "La descripción puede tener hasta 300 caracteres.")]
    public string? Descripcion { get; set; }
}

public class ReclamarObjetoVM
{
    public int ObjetoId { get; set; }

    [Required(ErrorMessage = "Ingresa tu código de alumno.")]
    [RegularExpression("^[0-9]{10}$", ErrorMessage = "El código de alumno tiene 10 dígitos.")]
    public string CodigoAlumno { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu nombre completo.")]
    [StringLength(120)]
    public string NombreCompleto { get; set; } = "";

    [Required(ErrorMessage = "Describe el objeto para verificar que es tuyo.")]
    [StringLength(500, MinimumLength = 15, ErrorMessage = "Describe el objeto con más detalle (entre 15 y 500 caracteres).")]
    public string Descripcion { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu número de WhatsApp.")]
    [RegularExpression(Services.WhatsApp.Patron, ErrorMessage = "El WhatsApp debe tener 9 dígitos y empezar con 9.")]
    public string WhatsApp { get; set; } = "";
}
