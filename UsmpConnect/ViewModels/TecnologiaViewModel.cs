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
}
