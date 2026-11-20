using LINDE.Registro.Api.Data;
using LINDE.Registro.Api.DTOs.Productores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LINDE.Registro.Api.Controllers;

/// <summary>
/// Gestión de productores (CU-02).
/// </summary>
[ApiController]
[Route("api/productores")]
[Produces("application/json")]
public class ProductoresController(LindeDbContext db) : ControllerBase
{
    /// <summary>
    /// Lista todos los productores ordenados por apellido y nombre.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductorRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductorRespuestaDto>>> Listar(CancellationToken ct)
    {
        var productores = await db.Productores
            .AsNoTracking()
            .OrderBy(p => p.Apellido)
            .ThenBy(p => p.Nombre)
            .Select(p => new ProductorRespuestaDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Apellido = p.Apellido,
                Ci = p.Ci,
                Telefono = p.Telefono,
                FechaRegistro = p.FechaRegistro,
                CantidadPredios = p.Predios.Count
            })
            .ToListAsync(ct);

        return Ok(productores);
    }

    /// <summary>
    /// Obtiene un productor por su id.
    /// </summary>
    /// <param name="id">Id del productor.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductorRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductorRespuestaDto>> ObtenerPorId(int id, CancellationToken ct)
    {
        var productor = await db.Productores
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductorRespuestaDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Apellido = p.Apellido,
                Ci = p.Ci,
                Telefono = p.Telefono,
                FechaRegistro = p.FechaRegistro,
                CantidadPredios = p.Predios.Count
            })
            .FirstOrDefaultAsync(ct);

        if (productor is null)
            return NoEncontrado(id);

        return Ok(productor);
    }

    private NotFoundObjectResult NoEncontrado(int id) =>
        NotFound(new ProblemDetails
        {
            Title = "Productor no encontrado",
            Detail = $"No existe un productor con id {id}.",
            Status = StatusCodes.Status404NotFound
        });
}
