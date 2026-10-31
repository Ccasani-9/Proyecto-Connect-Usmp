using System.Text.RegularExpressions;

namespace UsmpConnect.Services.Agente;

/// <summary>Recupera el tema de continuaciones cortas sin arrastrarlo a preguntas nuevas.</summary>
public record ConsultaAsistente(string? Tema, string Contexto)
{
    public string Peticion => Tema == "apuntes"
        ? "Busca en el catálogo de apuntes estos temas: " + TerminosApuntes() +
          ". Devuelve los materiales encontrados o alternativas reales. No des clases ni escribas código."
        : Contexto;

    public string TerminosApuntes()
    {
        // Aísla la parte válida de una petición mixta antes de que el modelo redacte.
        var ultima = Contexto.Split('\n').Last();
        var tema = Regex.Match(ultima,
            @"(?:apuntes?|resumen(?:es)?|material(?:es)?)\s+(?:de|sobre|para)\s+(.+?)(?=\s+(?:hazme|haz|pero|antes|despues)|$)",
            RegexOptions.IgnoreCase);
        if (tema.Success) return tema.Groups[1].Value.Trim(' ', '?', '¿', '.', ',');
        var limpio = Regex.Replace(ultima, @"^\s*(?:¿?)(?:enseñame|enséñame|quiero aprender|busca(?:me)?|tienes|dame)\s+", "", RegexOptions.IgnoreCase);
        if (BuscadorAlgolia.Contiene(Contexto, "python") && !BuscadorAlgolia.Contiene(limpio, "python"))
            limpio = "Python " + limpio;
        return limpio.Trim(' ', '?', '¿', '.', ',');
    }

    public static ConsultaAsistente Crear(string mensaje, IReadOnlyList<MensajeAsistente> historial)
    {
        var tema = DetectarTema(mensaje);
        if (tema is not null) return new(tema, mensaje);
        var palabras = Regex.Matches(mensaje, @"\p{L}+").Select(m => m.Value).ToArray();
        var seguimiento = mensaje.Length < 100 &&
            (Regex.IsMatch(mensaje, @"^\s*[¿?]*(y\s+)?(por|hasta|menos de|con|a)\s+(s[/\.]?\s*)?\d", RegexOptions.IgnoreCase)
             || palabras.Length == 1 || Regex.IsMatch(mensaje, @"^\s*¿?(y |otros|otras|más|mas|esos|esas|de esos)", RegexOptions.IgnoreCase));
        if (!seguimiento) return new(null, mensaje);
        var usuarios = historial.Where(m => m.Rol == "usuario").TakeLast(6).ToList();
        for (var i = usuarios.Count - 1; i >= 0; i--)
        {
            tema = DetectarTema(usuarios[i].Texto);
            if (tema is not null)
                return new(tema, string.Join("\n", usuarios.Skip(i).Select(m => m.Texto).Append(mensaje)));
        }
        return new(null, mensaje);
    }

    private static string? DetectarTema(string mensaje)
    {
        bool Incluye(params string[] textos) => textos.Any(t => BuscadorAlgolia.Contiene(mensaje, t));
        // Las peticiones de uso necesitan la guía, no el listado de publicaciones.
        if (Incluye("publicar", "subir", "editar", "eliminar", "como funciona", "manual", "reglamento", "horario", "calendario", "nota minima", "promedio")) return "guia";
        // Una petición mixta de apuntes y un ejercicio conserva la búsqueda válida.
        if (Incluye("apunte", "resumen", "material", "python", "programacion", "calculo", "integrales")) return "apuntes";
        if (Incluye("restaurante", "huarique", "almorz", "comer", "comida", "chifa", "pizza", "hamburgues", "ceviche", "cafeteria")) return "restaurantes";
        if (Incluye("marketplace", "en venta", "comprar", "vender")) return "marketplace";
        if (Incluye("curso", "catedra", "docente", "profesor")) return "catedra";
        if (Incluye("guia", "como ", "regla", "resena", "aprobar", "sustitutorio", "objetos perdidos", "calculadora")) return "guia";
        return null;
    }
}
