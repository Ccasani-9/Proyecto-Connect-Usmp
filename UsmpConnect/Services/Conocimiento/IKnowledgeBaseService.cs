namespace UsmpConnect.Services.Conocimiento;

public record FuenteConocimiento(string Fuente, int? Pagina, string Texto);
public record ResultadoConocimiento(string Modo, string Estado, IReadOnlyList<FuenteConocimiento> Fuentes,
    IReadOnlyList<string> Advertencias);

public interface IKnowledgeBaseService
{
    Task<ResultadoConocimiento> BuscarAsync(string consulta, int limite = 5, CancellationToken ct = default);
}
