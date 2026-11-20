namespace LINDE.Registro.Api.DTOs.Productores;

/// <summary>
/// Productor tal como lo devuelve la API.
/// </summary>
public class ProductorRespuestaDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Ci { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    /// <summary>Fecha de registro en UTC.</summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>Cantidad de predios registrados a su nombre.</summary>
    public int CantidadPredios { get; set; }
}
