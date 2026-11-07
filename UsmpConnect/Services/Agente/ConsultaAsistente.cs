using System.Text.RegularExpressions;

namespace UsmpConnect.Services.Agente;

/// <summary>Recupera el tema de continuaciones cortas sin arrastrarlo a preguntas nuevas.</summary>
public record ConsultaAsistente(string? Tema, string Contexto)
{
    public bool ParaAlmorzar => BuscadorAlgolia.Contiene(Contexto, "almorz");

    public DayOfWeek DiaConsulta
    {
        get
        {
            var dias = Regex.Matches(Contexto, @"\b(hoy|ma[nñ]ana)\b", RegexOptions.IgnoreCase);
            return dias.Count > 0 && !dias[^1].Value.Equals("hoy", StringComparison.OrdinalIgnoreCase)
                ? HoraPeru.Ahora.AddDays(1).DayOfWeek : HoraPeru.Ahora.DayOfWeek;
        }
    }

    public UsmpConnect.Models.CategoriaRestaurante? CategoriaSolicitada => Contexto.Split('\n')
        .Reverse().Select(CatalogoFiaPlugin.CategoriaDesdeTexto).FirstOrDefault(c => c.HasValue);

    public decimal? PresupuestoSolicitado
    {
        get
        {
            var coincidencias = Regex.Matches(Contexto, @"(?:menos de|hasta|por|presupuesto(?: de)?|con)\s*(?:s[/\.]?\s*)?(\d+(?:[.,]\d{1,2})?)", RegexOptions.IgnoreCase);
            return coincidencias.Count > 0 && decimal.TryParse(coincidencias[^1].Groups[1].Value.Replace(',', '.'),
                System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var valor) ? valor : null;
        }
    }

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
        if (!EsContinuacion(mensaje)) return new(tema, mensaje);
        var usuarios = historial.Where(m => m.Rol == "usuario").TakeLast(6).ToList();
        var inicio = usuarios.FindLastIndex(m => DetectarTema(m.Texto) is not null && !EsContinuacion(m.Texto));
        if (inicio < 0) inicio = usuarios.FindLastIndex(m => DetectarTema(m.Texto) is not null);
        if (inicio < 0) return new(tema, mensaje);
        var anterior = DetectarTema(usuarios[inicio].Texto);
        return tema is null || tema == anterior
            ? new(anterior, string.Join("\n", usuarios.Skip(inicio).Select(m => m.Texto).Append(mensaje)))
            : new(tema, mensaje);
    }

    private static bool EsContinuacion(string mensaje)
    {
        var palabras = Regex.Matches(mensaje, @"\p{L}+");
        return mensaje.Length < 100 && (palabras.Count == 1 ||
            Regex.IsMatch(mensaje, @"^\s*[¿?]*(y\s+)?(por|hasta|menos de|con|a)\s+(s[/\.]?\s*)?\d", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(mensaje, @"^\s*¿?(y |otros|otras|más|mas|esos|esas|de esos)", RegexOptions.IgnoreCase));
    }

    private static string? DetectarTema(string mensaje)
    {
        bool Incluye(params string[] textos) => textos.Any(t => BuscadorAlgolia.Contiene(mensaje, t));
        // Las peticiones de uso necesitan la guía, no el listado de publicaciones.
        if (Incluye("publicar", "subir", "editar", "eliminar", "como funciona", "manual", "reglamento", "horario", "calendario", "nota minima", "promedio")) return "guia";
        // Una petición mixta de apuntes y un ejercicio conserva la búsqueda válida.
        if (Incluye("apunte", "resumen", "material", "python", "programacion", "calculo", "integrales")) return "apuntes";
        if (Incluye("restaurante", "huarique", "almorz", "comer", "comida", "chifa", "pizza", "hamburgues", "ceviche", "cafeteria", "cafe", "pasteleria", "postres")) return "restaurantes";
        if (Incluye("marketplace", "en venta", "comprar", "vender")) return "marketplace";
        if (Incluye("curso", "catedra", "docente", "profesor")) return "catedra";
        if (Incluye("guia", "como ", "regla", "resena", "aprobar", "sustitutorio", "objetos perdidos", "calculadora")) return "guia";
        return null;
    }
}
