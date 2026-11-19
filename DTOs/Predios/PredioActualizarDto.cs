using System.ComponentModel.DataAnnotations;

namespace LINDE.Registro.Api.DTOs.Predios;

/// <summary>
/// Datos de entrada para actualizar un predio. No incluye el estado ni la fecha de registro.
/// </summary>
public class PredioActualizarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El municipio es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El municipio debe tener entre 2 y 100 caracteres.")]
    public string Municipio { get; set; } = string.Empty;

    [Required(ErrorMessage = "La superficie es obligatoria.")]
    [Range(typeof(decimal), "0.01", "1000000", ErrorMessage = "La superficie debe ser mayor a 0 ha.")]
    public decimal? SuperficieHa { get; set; }

    [Range(typeof(decimal), "0", "1000000", ErrorMessage = "El desmonte declarado no puede ser negativo.")]
    public decimal? DesmonteDeclaradoHa { get; set; }

    [Required(ErrorMessage = "El productor es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El id del productor debe ser mayor a 0.")]
    public int? ProductorId { get; set; }
}
