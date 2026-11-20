using System.ComponentModel.DataAnnotations;

namespace LINDE.Registro.Api.DTOs.Productores;

/// <summary>
/// Datos para registrar un productor.
/// </summary>
public class ProductorCrearDto
{
    /// <summary>Nombre del productor.</summary>
    /// <example>Carlos</example>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Apellido del productor.</summary>
    /// <example>Rojas</example>
    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres.")]
    public string? Apellido { get; set; }

    /// <summary>Cédula de identidad, solo dígitos.</summary>
    /// <example>7891234</example>
    [Required(ErrorMessage = "El CI es obligatorio.")]
    [RegularExpression(@"^\d{5,10}$", ErrorMessage = "El CI debe tener entre 5 y 10 dígitos.")]
    public string? Ci { get; set; }

    /// <summary>Teléfono de contacto (opcional).</summary>
    /// <example>70033445</example>
    [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
    public string? Telefono { get; set; }
}
