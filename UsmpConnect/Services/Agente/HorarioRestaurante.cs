using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace UsmpConnect.Services.Agente;

public static class HorarioRestaurante
{
    // Solo interpreta intervalos publicados reconocibles; null significa que debe confirmarse.
    public static bool? IncluyeAlmuerzo(string horario, DayOfWeek dia)
    {
        var dias = new[] { "lun", "mar", "mie", "jue", "vie", "sab", "dom" };
        var objetivo = dia == DayOfWeek.Sunday ? 6 : (int)dia - 1;
        var texto = string.Concat(horario.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)).ToLowerInvariant();
        var reconocidos = false;
        foreach (var bloque in texto.Split('·'))
        {
            var etiquetas = Regex.Matches(bloque, @"\b(lun|mar|mie|jue|vie|sab|dom)\b").Select(m => Array.IndexOf(dias, m.Value)).ToArray();
            var horas = Regex.Matches(bloque, @"(\d{1,2}):(\d{2})\s*(am|pm)");
            if (etiquetas.Length == 0 || horas.Count != 2) continue;
            reconocidos = true;
            var rango = etiquetas.Length == 2 && Regex.IsMatch(bloque, @"(?:lun|mar|mie|jue|vie|sab|dom)\s*[-–]\s*(?:lun|mar|mie|jue|vie|sab|dom)");
            var coincide = rango ? (etiquetas[0] <= etiquetas[1]
                ? objetivo >= etiquetas[0] && objetivo <= etiquetas[1]
                : objetivo >= etiquetas[0] || objetivo <= etiquetas[1]) : etiquetas.Contains(objetivo);
            if (!coincide) continue;
            int Minutos(Match m)
            {
                var h = int.Parse(m.Groups[1].Value);
                var min = int.Parse(m.Groups[2].Value);
                return h is < 1 or > 12 || min > 59 ? -1 : (h % 12 + (m.Groups[3].Value == "pm" ? 12 : 0)) * 60 + min;
            }
            var abre = Minutos(horas[0]); var cierra = Minutos(horas[1]);
            if (abre < 0 || cierra < 0) return null;
            if (cierra <= abre) cierra += 24 * 60;
            if (abre < 15 * 60 && cierra > 12 * 60) return true;
        }
        return reconocidos ? false : null;
    }
}
