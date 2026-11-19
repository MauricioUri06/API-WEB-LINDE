using LINDE.Registro.Api.Data;
using LINDE.Registro.Api.DTOs.Predios;
using LINDE.Registro.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LINDE.Registro.Api.Controllers;

/// <summary>
/// Gestión de predios agrícolas (recurso principal de la API).
/// </summary>
[ApiController]
[Route("api/predios")]
[Produces("application/json")]
public class PrediosController(LindeDbContext db) : ControllerBase
{
    /// <summary>
    /// Lista los predios, ordenados por nombre. Opcionalmente filtra por municipio.
    /// </summary>
    /// <param name="municipio">Municipio a filtrar (sin distinguir mayúsculas). Opcional.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="200">Lista de predios (puede estar vacía).</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<PredioRespuestaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PredioRespuestaDto>>> Listar(
        [FromQuery] string? municipio,
        CancellationToken ct)
    {
        var consulta = db.Predios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(municipio))
        {
            var filtro = municipio.Trim().ToLower();
            consulta = consulta.Where(p => p.Municipio.ToLower() == filtro);
        }

        var predios = await consulta
            .OrderBy(p => p.Nombre)
            .Select(p => new PredioRespuestaDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Municipio = p.Municipio,
                SuperficieHa = p.SuperficieHa,
                DesmonteDeclaradoHa = p.DesmonteDeclaradoHa,
                Estado = p.Estado,
                FechaRegistro = p.FechaRegistro,
                ProductorId = p.ProductorId,
                ProductorNombre = p.Productor.Nombre + " " + p.Productor.Apellido
            })
            .ToListAsync(ct);

        return Ok(predios);
    }

    /// <summary>
    /// Consulta un predio por su identificador.
    /// </summary>
    /// <param name="id">Identificador del predio.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="200">Predio encontrado.</response>
    /// <response code="404">No existe un predio con ese id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PredioRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PredioRespuestaDto>> ObtenerPorId(int id, CancellationToken ct)
    {
        var predio = await db.Predios
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PredioRespuestaDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Municipio = p.Municipio,
                SuperficieHa = p.SuperficieHa,
                DesmonteDeclaradoHa = p.DesmonteDeclaradoHa,
                Estado = p.Estado,
                FechaRegistro = p.FechaRegistro,
                ProductorId = p.ProductorId,
                ProductorNombre = p.Productor.Nombre + " " + p.Productor.Apellido
            })
            .FirstOrDefaultAsync(ct);

        if (predio is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Predio no encontrado",
                Detail = $"No existe un predio con id {id}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(predio);
    }
}
