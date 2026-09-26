using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UsmpConnect.Models;

public enum CategoriaRestaurante
{
    [Display(Name = "Menú Criollo")]
    MenuCriollo,
    [Display(Name = "Comida Rápida")]
    ComidaRapida,
    [Display(Name = "Chifa")]
    Chifa,
    [Display(Name = "Cafetería & Huarique")]
    CafeteriaHuarique,
    [Display(Name = "Pastelería & Postres")]
    PasteleriaPostres,
    [Display(Name = "Saludable & Bowls")]
    Saludable
}

public enum RangoPrecio
{
    [Display(Name = "Económico (S/ 8 - S/ 12)")]
    Economico,
    [Display(Name = "Medio (S/ 13 - S/ 20)")]
    Medio,
    [Display(Name = "Premium (S/ 21+)")]
    Premium
}

public class Restaurante
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    public CategoriaRestaurante Categoria { get; set; }

    public RangoPrecio Rango { get; set; }

    [Required, StringLength(150)]
    public string Direccion { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string DistanciaTexto { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string RangoPrecios { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string HorarioAtencion { get; set; } = string.Empty;

    [StringLength(20)]
    public string? TelefonoWhatsApp { get; set; }

    [Required, StringLength(255)]
    public string FotoUrl { get; set; } = string.Empty;

    public double Latitud { get; set; }
    public double Longitud { get; set; }

    public double CalificacionPromedio { get; set; }
    public int TotalResenas { get; set; }

    public ICollection<PlatoRestaurante> Platos { get; set; } = new List<PlatoRestaurante>();
    public ICollection<ResenaRestaurante> Resenas { get; set; } = new List<ResenaRestaurante>();
}

public class PlatoRestaurante
{
    public int Id { get; set; }

    public int RestauranteId { get; set; }
    public Restaurante Restaurante { get; set; } = null!;

    [Required, StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [Column(TypeName = "REAL")]
    public decimal Precio { get; set; }

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public bool EsPopular { get; set; }
}

public class ResenaRestaurante
{
    public int Id { get; set; }

    public int RestauranteId { get; set; }
    public Restaurante Restaurante { get; set; } = null!;

    [Required]
    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;

    [Range(1, 5)]
    public int Puntuacion { get; set; }

    [Required, StringLength(500)]
    public string Comentario { get; set; } = string.Empty;

    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
}
