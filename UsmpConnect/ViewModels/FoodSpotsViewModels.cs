using System.ComponentModel.DataAnnotations;
using UsmpConnect.Models;

namespace UsmpConnect.ViewModels;

public record RestauranteItemVM(
    int Id,
    string Nombre,
    string Descripcion,
    string CategoriaTexto,
    CategoriaRestaurante Categoria,
    RangoPrecio Rango,
    string Direccion,
    string DistanciaTexto,
    string RangoPrecios,
    string HorarioAtencion,
    string? TelefonoWhatsApp,
    string FotoUrl,
    double Latitud,
    double Longitud,
    double CalificacionPromedio,
    int TotalResenas,
    List<PlatoItemVM> PlatosPopulares
);

public record PlatoItemVM(
    int Id,
    string Nombre,
    decimal Precio,
    string? Descripcion,
    bool EsPopular
);

public record ResenaItemVM(
    int Id,
    string Autor,
    string Iniciales,
    int Puntuacion,
    string Comentario,
    string FechaRelativa
);

public class FoodSpotsIndexVM
{
    public List<RestauranteItemVM> Restaurantes { get; set; } = new();
    public CategoriaRestaurante? CategoriaActual { get; set; }
    public string? Busqueda { get; set; }
    public int TotalLugares => Restaurantes.Count;
    public string FuenteDatos { get; set; } = "SQLite"; // "Redis Cache (Hit)" o "SQLite"
}

public class CrearResenaVM
{
    [Required]
    public int RestauranteId { get; set; }

    [Range(1, 5, ErrorMessage = "La puntuación debe ser entre 1 y 5 estrellas.")]
    public int Puntuacion { get; set; } = 5;

    [Required(ErrorMessage = "Por favor escribe un breve comentario de tu experiencia."), StringLength(500, MinimumLength = 5, ErrorMessage = "El comentario debe tener entre 5 y 500 caracteres.")]
    public string Comentario { get; set; } = string.Empty;
}
