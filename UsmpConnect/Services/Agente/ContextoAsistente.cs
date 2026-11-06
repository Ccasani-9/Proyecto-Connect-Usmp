namespace UsmpConnect.Services.Agente;

public record TarjetaAsistente(string Titulo, string Detalle, string Url, string Tipo);
public record MensajeAsistente(string Rol, string Texto, IReadOnlyList<TarjetaAsistente>? Tarjetas = null);

public sealed class ContextoAsistente
{
    public List<TarjetaAsistente> Tarjetas { get; } = [];
    public void Agregar(TarjetaAsistente tarjeta)
    {
        if (Tarjetas.Count < 8 && !Tarjetas.Any(t => t.Url == tarjeta.Url)) Tarjetas.Add(tarjeta);
    }
}

public interface IAsistenteService
{
    bool EstaConfigurado { get; }
    IAsyncEnumerable<string> ResponderAsync(string mensaje, IReadOnlyList<MensajeAsistente> historial,
        ContextoAsistente contexto, CancellationToken ct);
}
