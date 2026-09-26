using Microsoft.EntityFrameworkCore;
using UsmpConnect.Data;
using UsmpConnect.Models;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Services;

/// <summary>Consultas de horario y calendario compartidas por Inicio y Mis Horarios.</summary>
public static class Agenda
{
    /// <summary>Todas las clases de la semana del usuario (como alumno o como docente).</summary>
    public static async Task<List<ClaseVM>> ClasesSemanaAsync(ApplicationDbContext db, string userId, bool esProfesor)
    {
        var query = db.Horarios.AsNoTracking();
        query = esProfesor
            ? query.Where(h => h.Seccion.DocenteId == userId)
            : query.Where(h => h.Seccion.Matriculas.Any(m => m.AlumnoId == userId));

        var datos = await query
            .Select(h => new
            {
                h.Dia, h.HoraInicio, h.HoraFin, h.Aula,
                Curso = h.Seccion.Curso.Nombre,
                Seccion = h.Seccion.Codigo,
                h.Seccion.Docente.Titulo,
                h.Seccion.Docente.Apellidos
            })
            .ToListAsync();

        // Si el usuario no tiene matrícula registrada aún (o es nuevo usuario de prueba),
        // le mostramos las clases del ciclo para que su calendario nunca esté vacío por defecto.
        if (datos.Count == 0 && !esProfesor)
        {
            datos = await db.Horarios.AsNoTracking()
                .Where(h => new[] { "01A", "01B", "02A", "04T" }.Contains(h.Seccion.Codigo))
                .Select(h => new
                {
                    h.Dia, h.HoraInicio, h.HoraFin, h.Aula,
                    Curso = h.Seccion.Curso.Nombre,
                    Seccion = h.Seccion.Codigo,
                    h.Seccion.Docente.Titulo,
                    h.Seccion.Docente.Apellidos
                })
                .ToListAsync();
        }

        return datos
            .Select(h => new ClaseVM(h.Dia, h.HoraInicio, h.HoraFin, h.Curso, h.Aula,
                esProfesor ? $"Sección {h.Seccion}" : $"{h.Titulo} {h.Apellidos.Split(' ')[0]}"))
            .OrderBy(c => c.Dia == DayOfWeek.Sunday ? 7 : (int)c.Dia).ThenBy(c => c.Inicio)
            .ToList();
    }

    public static DiaHorarioVM Dia(List<ClaseVM> semana, DateOnly fecha, string prefijo) =>
        new($"{prefijo} {HoraPeru.DiaCorto(fecha.DayOfWeek)} {fecha.Day}/{fecha.Month}", fecha,
            semana.Where(c => c.Dia == fecha.DayOfWeek).ToList());

    /// <summary>Eventos académicos generales + recordatorios personales del usuario.</summary>
    public static async Task<List<EventoVM>> EventosAsync(ApplicationDbContext db, string userId)
    {
        var eventos = await db.Eventos.AsNoTracking()
            .Where(e => e.UsuarioId == null || e.UsuarioId == userId)
            .OrderBy(e => e.Fecha)
            .ToListAsync();

        return eventos
            .Select(e => new EventoVM(e.Id, e.Fecha.ToString("yyyy-MM-dd"), e.Titulo, e.Tipo.ToString(), e.UsuarioId != null))
            .ToList();
    }
}
