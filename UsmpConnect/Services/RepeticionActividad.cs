namespace UsmpConnect.Services;

/// <summary>Expande recordatorios semanales acotados; cada fecha se guarda como actividad independiente.</summary>
public static class RepeticionActividad
{
    public static readonly string[] Emojis = ["📌", "📚", "✏️", "📝", "🎯", "💻", "🧪", "👥", "🎓", "⚽", "🎉", "☕"];

    public static List<DateOnly> Fechas(DateOnly inicio, bool semanal, DateOnly? hasta, int[]? dias)
    {
        if (inicio == default) throw new ArgumentException("Selecciona una fecha válida.");
        if (!semanal) return [inicio];
        if (hasta is null || hasta < inicio || hasta.Value.DayNumber - inicio.DayNumber > 366)
            throw new ArgumentException("La repetición debe terminar entre la fecha inicial y un año después.");
        if (dias is null || dias.Length == 0 || dias.Any(d => d < 0 || d > 6))
            throw new ArgumentException("Selecciona al menos un día de la semana.");
        var fechas = new List<DateOnly>();
        for (var n = inicio.DayNumber; n <= hasta.Value.DayNumber; n++)
        {
            var fecha = DateOnly.FromDayNumber(n);
            if (dias.Contains((int)fecha.DayOfWeek)) fechas.Add(fecha);
        }
        if (fechas.Count == 0) throw new ArgumentException("No hay días seleccionados dentro de ese intervalo.");
        return fechas;
    }
}
