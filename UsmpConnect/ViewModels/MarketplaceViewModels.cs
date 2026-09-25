using System.ComponentModel.DataAnnotations;
using UsmpConnect.Models;

namespace UsmpConnect.ViewModels;

public class MarketplaceIndexViewModel
{
    public List<ArticuloMarketplace> Articulos { get; set; } = new();
    public string? Busqueda { get; set; }
    public CategoriaMarketplace? CategoriaFiltro { get; set; }
}

public class PublicarArticuloInputModel
{
    [Required(ErrorMessage = "El título del artículo es obligatorio.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 120 caracteres.")]
    [Display(Name = "Título del artículo")]
    public string Titulo { get; set; } = "";

    [Required(ErrorMessage = "Selecciona una categoría.")]
    [Display(Name = "Categoría")]
    public CategoriaMarketplace Categoria { get; set; }

    [Display(Name = "Precio sugerido (S/)")]
    [Range(0, 99999, ErrorMessage = "El precio debe ser mayor o igual a 0.")]
    public decimal? Precio { get; set; }

    [Required(ErrorMessage = "Selecciona el estado del ítem.")]
    [Display(Name = "Estado del ítem")]
    public EstadoItem EstadoItem { get; set; } = EstadoItem.Usado;

    [Required(ErrorMessage = "Ingresa un número de WhatsApp para contacto.")]
    [RegularExpression(@"^[0-9+ ]{8,15}$", ErrorMessage = "Ingresa un número de WhatsApp válido.")]
    [Display(Name = "Número de WhatsApp")]
    public string WhatsApp { get; set; } = "";

    [StringLength(250, ErrorMessage = "Las notas de entrega no pueden superar los 250 caracteres.")]
    [Display(Name = "Notas de entrega en campus")]
    public string? NotasEntrega { get; set; }
}
