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

    /// <summary>
    /// Registra un predio nuevo. El estado inicial es Registrado.
    /// </summary>
    /// <param name="dto">Datos del predio.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="201">Predio creado. El encabezado Location apunta al nuevo recurso.</response>
    /// <response code="400">Datos inválidos o el productor indicado no existe.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PredioRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PredioRespuestaDto>> Crear(PredioCrearDto dto, CancellationToken ct)
    {
        var productorId = dto.ProductorId!.Value;

        var productor = await db.Productores
            .AsNoTracking()
            .Where(p => p.Id == productorId)
            .Select(p => new { p.Nombre, p.Apellido })
            .FirstOrDefaultAsync(ct);

        if (productor is null)
        {
            ModelState.AddModelError(nameof(dto.ProductorId), $"No existe un productor con id {productorId}.");
            return ValidationProblem(ModelState);
        }

        var predio = new Predio
        {
            Nombre = dto.Nombre.Trim(),
            Municipio = dto.Municipio.Trim(),
            SuperficieHa = dto.SuperficieHa!.Value,
            DesmonteDeclaradoHa = dto.DesmonteDeclaradoHa,
            Estado = EstadoPredio.Registrado,
            FechaRegistro = DateTime.UtcNow,
            ProductorId = productorId
        };

        db.Predios.Add(predio);
        await db.SaveChangesAsync(ct);

        var respuesta = new PredioRespuestaDto
        {
            Id = predio.Id,
            Nombre = predio.Nombre,
            Municipio = predio.Municipio,
            SuperficieHa = predio.SuperficieHa,
            DesmonteDeclaradoHa = predio.DesmonteDeclaradoHa,
            Estado = predio.Estado,
            FechaRegistro = predio.FechaRegistro,
            ProductorId = predio.ProductorId,
            ProductorNombre = productor.Nombre + " " + productor.Apellido
        };

        return CreatedAtAction(nameof(ObtenerPorId), new { id = predio.Id }, respuesta);
    }

    /// <summary>
    /// Actualiza los datos de un predio. No modifica su estado ni su fecha de registro.
    /// </summary>
    /// <param name="id">Identificador del predio.</param>
    /// <param name="dto">Nuevos datos del predio.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="204">Predio actualizado.</response>
    /// <response code="400">Datos inválidos o el productor indicado no existe.</response>
    /// <response code="404">No existe un predio con ese id.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, PredioActualizarDto dto, CancellationToken ct)
    {
        var predio = await db.Predios.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (predio is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Predio no encontrado",
                Detail = $"No existe un predio con id {id}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var productorId = dto.ProductorId!.Value;

        if (!await db.Productores.AnyAsync(p => p.Id == productorId, ct))
        {
            ModelState.AddModelError(nameof(dto.ProductorId), $"No existe un productor con id {productorId}.");
            return ValidationProblem(ModelState);
        }

        predio.Nombre = dto.Nombre.Trim();
        predio.Municipio = dto.Municipio.Trim();
        predio.SuperficieHa = dto.SuperficieHa!.Value;
        predio.DesmonteDeclaradoHa = dto.DesmonteDeclaradoHa;
        predio.ProductorId = productorId;

        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>
    /// Elimina un predio.
    /// </summary>
    /// <param name="id">Identificador del predio.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="204">Predio eliminado.</response>
    /// <response code="404">No existe un predio con ese id.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        var predio = await db.Predios.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (predio is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Predio no encontrado",
                Detail = $"No existe un predio con id {id}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        db.Predios.Remove(predio);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>
    /// Recurso relacionado: lista los predios de un productor.
    /// </summary>
    /// <param name="productorId">Identificador del productor.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="200">Predios del productor (puede estar vacía).</response>
    /// <response code="404">No existe un productor con ese id.</response>
    [HttpGet("/api/productores/{productorId:int}/predios")]
    [ProducesResponseType(typeof(List<PredioRespuestaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PredioRespuestaDto>>> ListarPorProductor(
        int productorId,
        CancellationToken ct)
    {
        if (!await db.Productores.AnyAsync(p => p.Id == productorId, ct))
        {
            return NotFound(new ProblemDetails
            {
                Title = "Productor no encontrado",
                Detail = $"No existe un productor con id {productorId}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var predios = await db.Predios
            .AsNoTracking()
            .Where(p => p.ProductorId == productorId)
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
}
