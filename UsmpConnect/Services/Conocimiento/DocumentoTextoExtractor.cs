using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace UsmpConnect.Services.Conocimiento;

public record PaginaDocumento(int? Numero, string Texto);

public sealed class DocumentoTextoExtractor
{
    public const int MaxBytes = 5 * 1024 * 1024;
    public const int MaxCaracteres = 200_000;

    public async Task<IReadOnlyList<PaginaDocumento>> ExtraerAsync(Stream archivo, string nombre, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(nombre).ToLowerInvariant();
        if (extension is not (".pdf" or ".md" or ".txt"))
            throw new InvalidDataException("Solo se admiten PDF, Markdown y TXT.");

        // Lectura acotada también para streams sin Length.
        using var contenido = new MemoryStream();
        var buffer = new byte[8192];
        int leidos;
        while ((leidos = await archivo.ReadAsync(buffer, ct)) > 0)
        {
            if (contenido.Length + leidos > MaxBytes)
                throw new InvalidDataException("El documento supera los 5 MB.");
            contenido.Write(buffer, 0, leidos);
        }
        contenido.Position = 0;
        var paginas = new List<PaginaDocumento>();
        var total = 0;
        void Agregar(int? pagina, string texto)
        {
            texto = texto.Replace('\0', ' ').Trim();
            total += texto.Length;
            if (total > MaxCaracteres) throw new InvalidDataException("El documento supera los 200.000 caracteres.");
            if (texto.Length > 0) paginas.Add(new(pagina, texto));
        }

        if (extension == ".pdf")
        {
            try
            {
                using var pdf = PdfDocument.Open(contenido);
                foreach (var pagina in pdf.GetPages())
                {
                    ct.ThrowIfCancellationRequested();
                    Agregar(pagina.Number, ContentOrderTextExtractor.GetText(pagina));
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (InvalidDataException) { throw; }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                throw new InvalidDataException("No se pudo leer el PDF. Comprueba que sea válido y no esté protegido.", ex);
            }
        }
        else
        {
            try
            {
                using var lector = new StreamReader(contenido, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: true);
                Agregar(null, await lector.ReadToEndAsync(ct));
            }
            catch (DecoderFallbackException ex) { throw new InvalidDataException("Guarda el documento como texto UTF-8.", ex); }
        }
        if (paginas.Count == 0)
            throw new InvalidDataException("El documento no contiene texto extraíble. Los PDF escaneados necesitan OCR previo.");
        return paginas;
    }
}
