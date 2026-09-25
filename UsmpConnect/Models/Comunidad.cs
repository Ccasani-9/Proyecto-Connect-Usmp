using System.ComponentModel.DataAnnotations;

namespace UsmpConnect.Models;

public enum TipoEvento
{
    Institucional,
    Evaluacion,
    Entrega,
    Personal
}

/// <summary>
/// Evento del calendario. Si <see cref="UsuarioId"/> es null, es un evento académico
/// visible para todos; si no, es un recordatorio personal.
/// </summary>
public class EventoAcademico
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Titulo { get; set; } = "";

    public DateOnly Fecha { get; set; }

    public TipoEvento Tipo { get; set; }

    public string? UsuarioId { get; set; }
    public ApplicationUser? Usuario { get; set; }
}

public class Notificacion
{
    public int Id { get; set; }

    public string UsuarioId { get; set; } = "";
    public ApplicationUser Usuario { get; set; } = null!;

    [MaxLength(300)]
    public string Mensaje { get; set; } = "";

    [MaxLength(200)]
    public string? Url { get; set; }

    [MaxLength(40)]
    public string Icono { get; set; } = "bell";

    public bool Leida { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}

/// <summary>Caso reportado desde el banner "¿Tuviste algún inconveniente con tu matrícula?".</summary>
public class CasoMatricula
{
    public int Id { get; set; }

    public string UsuarioId { get; set; } = "";
    public ApplicationUser Usuario { get; set; } = null!;

    [MaxLength(60)]
    public string Tipo { get; set; } = "";

    [MaxLength(1000)]
    public string Descripcion { get; set; } = "";

    [MaxLength(30)]
    public string Estado { get; set; } = "Recibido";

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
