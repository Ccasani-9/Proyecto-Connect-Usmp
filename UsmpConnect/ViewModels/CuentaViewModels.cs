using System.ComponentModel.DataAnnotations;

namespace UsmpConnect.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu correo institucional")]
    [EmailAddress(ErrorMessage = "Correo no válido")]
    [Display(Name = "Correo institucional")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu contraseña")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    [Display(Name = "Mantener sesión iniciada")]
    public bool Recordarme { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegistroViewModel
{
    [Required]
    public string Tipo { get; set; } = "Alumno";

    [Required(ErrorMessage = "Ingresa tus nombres")]
    [StringLength(60)]
    public string Nombres { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tus apellidos")]
    [StringLength(60)]
    public string Apellidos { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu código de alumno")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "El código universitario debe tener 10 dígitos")]
    [Display(Name = "Código de alumno")]
    public string CodigoAlumno { get; set; } = "";

    [Required(ErrorMessage = "Selecciona tu escuela")]
    public string Escuela { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu correo institucional")]
    [EmailAddress(ErrorMessage = "Correo no válido")]
    [Display(Name = "Correo institucional")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Crea una contraseña")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarPassword { get; set; } = "";

    public static readonly string[] Escuelas =
    [
        "Ing. de Computación y Sistemas", "Ing. Civil", "Ing. Industrial", "Ing. Electrónica",
        "Arquitectura", "Derecho", "Administración", "Contabilidad y Finanzas",
        "Medicina Humana", "Psicología", "Ciencias de la Comunicación", "Turismo y Hotelería", "Humanidades"
    ];
}
