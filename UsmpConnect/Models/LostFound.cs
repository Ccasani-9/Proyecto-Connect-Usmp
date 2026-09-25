using System.ComponentModel.DataAnnotations;

namespace UsmpConnect.Models;

/// <summary>Encontrado = alguien lo halló y busca al dueño. Perdido = el dueño lo está buscando.</summary>
public enum TipoReporte
{
    Encontrado,
    Perdido
}

public enum CategoriaObjeto
{
    [Display(Name = "Carnés Universitarios")] Carne,
    [Display(Name = "Electrónicos / Audífonos")] Electronico,
    [Display(Name = "Ropa / Casacas")] Ropa,
    [Display(Name = "Cuadernos / Otros")] Otros
}

/// <summary>Dónde quedó el objeto encontrado mientras aparece su dueño.</summary>
public enum CustodiaObjeto
{
    [Display(Name = "En custodia de Seguridad")] Seguridad,
    [Display(Name = "Con el alumno reportante")] Reportante,
    [Display(Name = "En la Secretaría de la Facultad")] Secretaria
}

public enum EstadoObjeto
{
    Pendiente,
    [Display(Name = "Entregado a su dueño")] Devuelto
}

public enum EstadoReclamo
{
    Pendiente,
    Aprobado,
    Rechazado
}

public class ObjetoPerdido
{
    public static readonly string[] Pabellones =
    [
        "Pabellón A", "Pabellón B", "Pabellón C", "Biblioteca Central", "Cafetería Principal",
        "Laboratorios FIA", "Auditorio", "Losa deportiva", "Estacionamiento"
    ];

    public int Id { get; set; }

    public TipoReporte Tipo { get; set; }

    [MaxLength(100)]
    public string Titulo { get; set; } = "";

    public CategoriaObjeto Categoria { get; set; }

    [MaxLength(60)]
    public string Pabellon { get; set; } = "";

    [MaxLength(100)]
    public string Lugar { get; set; } = "";

    /// <summary>Cuándo se encontró (o perdió), en hora de Lima tal como la ingresó el alumno.</summary>
    public DateTime FechaHallazgo { get; set; }

    [MaxLength(200)]
    public string? FotoUrl { get; set; }

    /// <summary>Solo para objetos encontrados.</summary>
    public CustodiaObjeto? Custodia { get; set; }

    /// <summary>Celular de 9 dígitos, sin el +51. Obligatorio si el objeto está perdido.</summary>
    [MaxLength(9)]
    public string? WhatsApp { get; set; }

    [MaxLength(300)]
    public string? Descripcion { get; set; }

    public EstadoObjeto Estado { get; set; }

    public string ReportanteId { get; set; } = "";
    public ApplicationUser Reportante { get; set; } = null!;

    public DateTime FechaReporte { get; set; } = DateTime.UtcNow;

    public List<ReclamoObjeto> Reclamos { get; set; } = [];

    public string Ubicacion => string.IsNullOrWhiteSpace(Lugar) ? Pabellon : $"{Pabellon} — {Lugar}";
}

/// <summary>Solicitud de un alumno que dice ser el dueño de un objeto encontrado.</summary>
public class ReclamoObjeto
{
    public int Id { get; set; }

    public int ObjetoId { get; set; }
    public ObjetoPerdido Objeto { get; set; } = null!;

    public string UsuarioId { get; set; } = "";
    public ApplicationUser Usuario { get; set; } = null!;

    [MaxLength(10)]
    public string CodigoAlumno { get; set; } = "";

    [MaxLength(120)]
    public string NombreCompleto { get; set; } = "";

    /// <summary>Detalles que solo el dueño conoce (color, marca, contenido...).</summary>
    [MaxLength(500)]
    public string Descripcion { get; set; } = "";

    [MaxLength(9)]
    public string WhatsApp { get; set; } = "";

    public EstadoReclamo Estado { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}

/// <summary>Íconos, etiquetas y fechas de Lost &amp; Found para las vistas.</summary>
public static class LostFoundUi
{
    public static string Icono(this CategoriaObjeto c) => c switch
    {
        CategoriaObjeto.Carne => "person-vcard",
        CategoriaObjeto.Electronico => "headphones",
        CategoriaObjeto.Ropa => "bag",
        _ => "journal-bookmark"
    };

    /// <summary>"28 Ago 2026, 14:30"</summary>
    public static string FechaCorta(DateTime fecha)
    {
        var mes = Services.HoraPeru.Cultura.DateTimeFormat.GetAbbreviatedMonthName(fecha.Month).TrimEnd('.');
        return $"{fecha.Day} {char.ToUpper(mes[0])}{mes[1..]} {fecha.Year}, {fecha:HH:mm}";
    }
}
