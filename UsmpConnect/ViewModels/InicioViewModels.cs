namespace UsmpConnect.ViewModels;

public record ClaseVM(DayOfWeek Dia, TimeOnly Inicio, TimeOnly Fin, string Curso, string Aula, string Detalle)
{
    public string Rango => $"{Inicio:HH\\:mm} - {Fin:HH\\:mm}";
}

public record DiaHorarioVM(string Etiqueta, DateOnly Fecha, List<ClaseVM> Clases);

public record NotaCursoVM(int MatriculaId, string Curso, string Docente, int Creditos, decimal? PP, decimal? EP, decimal? EF)
{
    public decimal? Promedio => (PP.HasValue && EP.HasValue && EF.HasValue)
        ? Math.Round((PP.Value * 0.3m) + (EP.Value * 0.3m) + (EF.Value * 0.4m), 1)
        : (EP.HasValue && EF.HasValue)
            ? Math.Round((EP.Value + EF.Value) / 2m, 1)
            : null;

    /// <summary>
    /// Nota que el alumno necesita en el Examen Final para alcanzar la nota mínima aprobatoria (10.5).
    /// </summary>
    public decimal? EfParaAprobar => (PP.HasValue && EP.HasValue && !EF.HasValue)
        ? Math.Max(0m, Math.Round((10.5m - (PP.Value * 0.3m) - (EP.Value * 0.3m)) / 0.4m, 1))
        : null;
}

public record SeccionResumenVM(int SeccionId, string Curso, string Codigo, int Alumnos, int ConNotas, string Horario);

public record EventoVM(int Id, string Fecha, string Titulo, string Tipo, bool Personal);

public class CalendarioViewModel
{
    public List<ClaseVM> Clases { get; set; } = [];
    public List<EventoVM> Eventos { get; set; } = [];
    public DateOnly FechaHoy { get; set; }
}

public class InicioViewModel
{
    public string PrimerNombre { get; set; } = "";
    public string Saludo { get; set; } = "";
    public bool EsProfesor { get; set; }

    public List<NotaCursoVM> Notas { get; set; } = [];
    public List<SeccionResumenVM> Secciones { get; set; } = [];

    public DiaHorarioVM Hoy { get; set; } = null!;
    public DiaHorarioVM Manana { get; set; } = null!;

    public List<EventoVM> Eventos { get; set; } = [];
    public List<EventoVM> ProximosEventos { get; set; } = [];
    public DateOnly FechaHoy { get; set; }
}

public class AlumnoNotaVM
{
    public int MatriculaId { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal? EP { get; set; }
    public decimal? EF { get; set; }
}

public class NotasSeccionViewModel
{
    public int SeccionId { get; set; }
    public string Curso { get; set; } = "";
    public string CodigoCurso { get; set; } = "";
    public string Seccion { get; set; } = "";
    public List<AlumnoNotaVM> Alumnos { get; set; } = [];
}
