using System.ComponentModel.DataAnnotations;

namespace UsmpConnect.Models;

public enum CategoriaMarketplace
{
    [Display(Name = "Calculadoras / Hardware")]
    CalculadorasHardware,

    [Display(Name = "Libros y Guías")]
    LibrosGuias,

    [Display(Name = "Batas / Uniformes")]
    BatasUniformes,

    [Display(Name = "Alquiler de cuartos")]
    AlquilerCuartos,

    [Display(Name = "Otros")]
    Otros
}

public enum EstadoItem
{
    Usado,
    Nuevo,
    [Display(Name = "Donación")]
    Donacion,
    [Display(Name = "Como nuevo")]
    ComoNuevo
}

public enum EstadoPublicacion
{
    Disponible,
    Reservado,
    Vendido
}

/// <summary>
/// Publicación de compra, venta o donación en el Marketplace Comunitario.
/// </summary>
public class ArticuloMarketplace
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Titulo { get; set; } = "";

    public CategoriaMarketplace Categoria { get; set; }

    /// <summary>Null o 0 si es donación/gratis.</summary>
    public decimal? Precio { get; set; }

    public EstadoItem EstadoItem { get; set; } = EstadoItem.Usado;

    public EstadoPublicacion Estado { get; set; } = EstadoPublicacion.Disponible;

    /// <summary>Ruta de la imagen en /uploads/marketplace/ o null si usa placeholder.</summary>
    [MaxLength(250)]
    public string? ImagenUrl { get; set; }

    /// <summary>Número de WhatsApp para contacto directo (ej: 987654321).</summary>
    [MaxLength(20)]
    public string WhatsApp { get; set; } = "";

    /// <summary>Notas sobre entrega en el campus (ej: "Disponible en biblioteca de 2-6pm").</summary>
    [MaxLength(250)]
    public string? NotasEntrega { get; set; }

    public string VendedorId { get; set; } = "";
    public ApplicationUser Vendedor { get; set; } = null!;

    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

    public bool EsDonacion => EstadoItem == EstadoItem.Donacion || !Precio.HasValue || Precio.Value == 0;

    public string PrecioFormateado => EsDonacion 
        ? "Gratis" 
        : Categoria == CategoriaMarketplace.AlquilerCuartos 
            ? $"S/ {Precio:F2}/mes" 
            : $"S/ {Precio:F2}";
}
