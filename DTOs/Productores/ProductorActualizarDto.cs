using System.ComponentModel.DataAnnotations;

namespace LINDE.Registro.Api.DTOs.Productores;

/// <summary>
/// Datos para actualizar un productor existente.
/// </summary>
public class ProductorActualizarDto
{
    /// <summary>Nombre del productor.</summary>
    /// <example>Ana María</example>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Apellido del productor.</summary>
    /// <example>Mendoza</example>
    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres.")]
    public string? Apellido { get; set; }

    /// <summary>Cédula de identidad, solo dígitos.</summary>
    /// <example>4567891</example>
    [Required(ErrorMessage = "El CI es obligatorio.")]
    [RegularExpression(@"^\d{5,10}$", ErrorMessage = "El CI debe tener entre 5 y 10 dígitos.")]
    public string? Ci { get; set; }

    /// <summary>Teléfono de contacto (opcional).</summary>
    /// <example>70011223</example>
    [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
    public string? Telefono { get; set; }
}
