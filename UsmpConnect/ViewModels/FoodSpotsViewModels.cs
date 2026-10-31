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
    List<PlatoItemVM> PlatosPopulares,
    decimal? PrecioPromedioReferencial = null
)
{
    public bool EsCampus => (Direccion?.Contains("Campus FIA", StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (DistanciaTexto?.Contains("Dentro del Campus", StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (Nombre?.Contains("Cafetería Central FIA", StringComparison.OrdinalIgnoreCase) ?? false);
}

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
    string FechaRelativa,
    string? Sentimiento = null,
    double? ProbabilidadSentimiento = null,
    string? UsuarioId = null
);

public record RestauranteDetalleDto(
    int Id,
    string Nombre,
    string Descripcion,
    string Categoria,
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
    bool EsCampus,
    List<PlatoItemVM> Platos,
    List<ResenaItemVM> Resenas,
    decimal? PrecioPromedioReferencial = null
);

public record ResenaDto(
    int Id,
    int RestauranteId,
    string RestauranteNombre,
    string UsuarioId,
    string Autor,
    int Puntuacion,
    string Comentario,
    DateTime FechaUtc,
    double CalificacionPromedioRestaurante,
    int TotalResenasRestaurante,
    string? Sentimiento = null,
    double? ProbabilidadSentimiento = null
);

public enum EstadoOperacionResena
{
    Ok,
    NoEncontrada,
    Prohibida
}

public class GuardarResenaRequest
{
    [Range(1, 5, ErrorMessage = "La puntuación debe ser entre 1 y 5 estrellas.")]
    public int Puntuacion { get; set; } = 5;

    [Required(ErrorMessage = "Por favor escribe un breve comentario de tu experiencia.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "El comentario debe tener entre 5 y 500 caracteres.")]
    public string Comentario { get; set; } = string.Empty;
}

public class FoodSpotsIndexVM
{
    public List<RestauranteItemVM> Restaurantes { get; set; } = new();
    public List<RestauranteItemVM> Recomendados { get; set; } = new();
    public bool RecomendacionPersonalizada { get; set; }
    public CategoriaRestaurante? CategoriaActual { get; set; }
    public string? Busqueda { get; set; }
    public int TotalLugares => Restaurantes.Count;
    public string FuenteDatos { get; set; } = "SQLite"; // "Redis Cache (Hit)" o "SQLite"
    public string GoogleMapsApiKey { get; set; } = string.Empty;
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
