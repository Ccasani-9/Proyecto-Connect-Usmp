using System.Text;

namespace UsmpConnect.Services;

/// <summary>
/// Genera un PDF de una página con texto (fuente Helvetica). Se usa para los apuntes de ejemplo,
/// sin librerías externas.
/// </summary>
public static class PdfSimple
{
    private const int MaxCaracteresPorLinea = 90;

    public static byte[] Generar(string titulo, IEnumerable<string> parrafos)
    {
        var contenido = new StringBuilder();
        contenido.Append($"BT /F1 18 Tf 56 780 Td ({Escapar(titulo)}) Tj ET\n");
        contenido.Append("0.42 0.06 0.13 RG 2 w 56 768 m 539 768 l S\n"); // línea guinda bajo el título

        var y = 740;
        foreach (var linea in parrafos.SelectMany(Partir))
        {
            if (y < 60)
                break;
            contenido.Append($"BT /F1 11 Tf 56 {y} Td ({Escapar(linea)}) Tj ET\n");
            y -= 17;
        }

        var stream = contenido.ToString();
        string[] objetos =
        [
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>",
            $"<< /Length {Encoding.Latin1.GetByteCount(stream)} >>\nstream\n{stream}endstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"
        ];

        using var pdf = new MemoryStream();
        void Escribir(string texto) => pdf.Write(Encoding.Latin1.GetBytes(texto));

        Escribir("%PDF-1.4\n");
        var posiciones = new List<long>();
        for (var i = 0; i < objetos.Length; i++)
        {
            posiciones.Add(pdf.Position);
            Escribir($"{i + 1} 0 obj\n{objetos[i]}\nendobj\n");
        }

        var xref = pdf.Position;
        Escribir($"xref\n0 {objetos.Length + 1}\n0000000000 65535 f \n");
        foreach (var p in posiciones)
            Escribir($"{p:D10} 00000 n \n");
        Escribir($"trailer\n<< /Size {objetos.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");

        return pdf.ToArray();
    }

    /// <summary>Divide un párrafo en líneas que entran en la página. Un párrafo vacío deja una línea en blanco.</summary>
    private static IEnumerable<string> Partir(string parrafo)
    {
        if (string.IsNullOrWhiteSpace(parrafo))
        {
            yield return "";
            yield break;
        }

        var linea = new StringBuilder();
        foreach (var palabra in parrafo.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (linea.Length > 0 && linea.Length + palabra.Length + 1 > MaxCaracteresPorLinea)
            {
                yield return linea.ToString();
                linea.Clear();
            }
            if (linea.Length > 0)
                linea.Append(' ');
            linea.Append(palabra);
        }
        if (linea.Length > 0)
            yield return linea.ToString();
    }

    /// <summary>Escapa los caracteres especiales de PDF y cambia los que no existen en WinAnsi.</summary>
    private static string Escapar(string texto) => texto
        .Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)")
        .Replace('—', '-').Replace('–', '-').Replace('“', '"').Replace('”', '"').Replace('’', '\'')
        .Replace("…", "...");
}
