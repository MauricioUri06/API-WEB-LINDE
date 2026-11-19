using LINDE.Registro.Api.Models;

namespace LINDE.Registro.Api.DTOs.Predios;

/// <summary>
/// Datos de un predio que devuelve la API.
/// </summary>
public class PredioRespuestaDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Municipio { get; set; } = string.Empty;

    public decimal SuperficieHa { get; set; }

    public decimal? DesmonteDeclaradoHa { get; set; }

    /// <summary>Estado del predio, serializado como texto.</summary>
    public EstadoPredio Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int ProductorId { get; set; }

    /// <summary>Nombre y apellido del productor dueño del predio.</summary>
    public string ProductorNombre { get; set; } = string.Empty;
}
