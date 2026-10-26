using System.Security.Cryptography;
using System.Text;

namespace UsmpConnect.Services.Conocimiento;

public record FragmentoConocimiento(Guid Id, string Fuente, int? Pagina, string Texto);
public record CorpusConocimiento(string Version, IReadOnlyList<FragmentoConocimiento> Fragmentos, IReadOnlyList<string> Advertencias);

/// <summary>Solo lee fuentes públicas preparadas por el equipo; no incluye archivos subidos por alumnos.</summary>
public sealed class CatalogoConocimiento(IWebHostEnvironment entorno, DocumentoTextoExtractor extractor,
    ILogger<CatalogoConocimiento> logger)
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private CorpusConocimiento? _corpus;

    public async Task<CorpusConocimiento> ObtenerAsync(CancellationToken ct = default)
    {
        if (_corpus is not null) return _corpus;
        await _gate.WaitAsync(ct);
        try
        {
            if (_corpus is not null) return _corpus;
            var carpeta = Path.Combine(entorno.ContentRootPath, "Data", "Conocimiento");
            var fragmentos = new List<FragmentoConocimiento>();
            var avisos = new List<string>();
            var archivos = Directory.Exists(carpeta)
                ? Directory.GetFiles(carpeta).Where(f => Path.GetExtension(f).ToLowerInvariant() is ".pdf" or ".md" or ".txt")
                    .Order(StringComparer.Ordinal).ToArray() : [];
            if (archivos.Length > 32) throw new InvalidDataException("El corpus admite hasta 32 documentos.");
            var total = 0;
            foreach (var ruta in archivos)
            {
                ct.ThrowIfCancellationRequested();
                var nombre = Path.GetFileName(ruta);
                try
                {
                    if (new FileInfo(ruta).Length > DocumentoTextoExtractor.MaxBytes)
                        throw new InvalidDataException("El documento supera los 5 MB.");
                    await using var archivo = File.OpenRead(ruta);
                    var paginas = await extractor.ExtraerAsync(archivo, nombre, ct);
                    var nuevos = Fragmentar(nombre, paginas);
                    var caracteres = paginas.Sum(p => p.Texto.Length);
                    if (total + caracteres > 1_000_000)
                        throw new InvalidDataException("El corpus supera 1.000.000 de caracteres.");
                    fragmentos.AddRange(nuevos);
                    total += caracteres;
                }
                catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
                {
                    avisos.Add($"No se cargó {nombre}: revisa el formato y los límites del documento.");
                    logger.LogWarning("No se cargó una fuente de conocimiento ({Tipo}).", ex.GetType().Name);
                }
            }
            var version = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                string.Join('|', fragmentos.Select(f => f.Id))))).ToLowerInvariant();
            return _corpus = new(version, fragmentos.AsReadOnly(), avisos.AsReadOnly());
        }
        finally { _gate.Release(); }
    }

    public static IReadOnlyList<FragmentoConocimiento> Fragmentar(string fuente, IReadOnlyList<PaginaDocumento> paginas)
    {
        var resultado = new List<FragmentoConocimiento>();
        foreach (var pagina in paginas)
        {
            for (var inicio = 0; inicio < pagina.Texto.Length; inicio += 850)
            {
                var texto = pagina.Texto.Substring(inicio, Math.Min(1000, pagina.Texto.Length - inicio)).Trim();
                if (texto.Length > 0)
                {
                    var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{fuente}\n{pagina.Numero}\n{inicio}\n{texto}"));
                    resultado.Add(new(new Guid(hash.AsSpan(0, 16)), fuente, pagina.Numero, texto));
                    if (resultado.Count > 250) throw new InvalidDataException("El documento supera los 250 fragmentos.");
                }
                if (inicio + 1000 >= pagina.Texto.Length) break;
            }
        }
        return resultado.AsReadOnly();
    }
}
