using UsmpConnect.Services;

namespace UsmpConnect.ViewModels;

public record ServicioStatusVM(
    string Nombre,
    string Categoria,
    string Proveedor,
    bool Conectado,
    string EstadoTexto,
    string Icono,
    string ColorBadge,
    string DescripcionUso
);

public class TecnologiaViewModel
{
    public List<ServicioStatusVM> Servicios { get; set; } = new();
    public int TotalTablasSQLite { get; set; }
    public int TotalUsuarios { get; set; }
    public int TotalRestaurantes { get; set; }
    public int TotalApuntes { get; set; }
    public int TotalArticulos { get; set; }
    public MetricasEntrenamientoSentimiento? MetricasSentimiento { get; set; }
    public string RutaModeloSentimiento { get; set; } = string.Empty;
    public MetricasEntrenamientoRecomendacion? MetricasRecomendacion { get; set; }
    public string RutaModeloRecomendacion { get; set; } = string.Empty;
}
