using System.Collections.Concurrent;

namespace UsmpConnect.Services;

public record UltimaOperacionIntegracion(bool Correcta, string Descripcion, DateTime FechaUtc);

/// <summary>Observaciones de este proceso; no confundir con un chequeo activo de disponibilidad.</summary>
public sealed class EstadoIntegraciones
{
    private readonly ConcurrentDictionary<string, UltimaOperacionIntegracion> _operaciones = new();
    public void Registrar(string servicio, bool correcta, string descripcion)
        => _operaciones[servicio] = new(correcta, descripcion, DateTime.UtcNow);
    public UltimaOperacionIntegracion? Obtener(string servicio)
        => _operaciones.TryGetValue(servicio, out var ultima) ? ultima : null;
}
