using LINDE.Registro.Api.Data;
using LINDE.Registro.Api.DTOs.Productores;
using LINDE.Registro.Api.Models;
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

    /// <summary>
    /// Registra un nuevo productor.
    /// </summary>
    /// <param name="dto">Datos del productor.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="201">Productor creado; el encabezado Location apunta al recurso.</response>
    /// <response code="400">Datos inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductorRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductorRespuestaDto>> Crear(ProductorCrearDto dto, CancellationToken ct)
    {
        var ci = dto.Ci!.Trim();
        if (await db.Productores.AnyAsync(p => p.Ci == ci, ct))
        {
            ModelState.AddModelError(nameof(dto.Ci), "Ya existe un productor con ese CI.");
            return ValidationProblem(ModelState);
        }

        var productor = new Productor
        {
            Nombre = dto.Nombre!.Trim(),
            Apellido = dto.Apellido!.Trim(),
            Ci = ci,
            Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
            FechaRegistro = DateTime.UtcNow
        };

        db.Productores.Add(productor);
        await db.SaveChangesAsync(ct);

        var respuesta = new ProductorRespuestaDto
        {
            Id = productor.Id,
            Nombre = productor.Nombre,
            Apellido = productor.Apellido,
            Ci = productor.Ci,
            Telefono = productor.Telefono,
            FechaRegistro = productor.FechaRegistro,
            CantidadPredios = 0
        };

        return CreatedAtAction(nameof(ObtenerPorId), new { id = productor.Id }, respuesta);
    }

    /// <summary>
    /// Actualiza los datos de un productor.
    /// </summary>
    /// <param name="id">Id del productor.</param>
    /// <param name="dto">Datos nuevos.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, ProductorActualizarDto dto, CancellationToken ct)
    {
        var productor = await db.Productores.FindAsync([id], ct);
        if (productor is null)
            return NoEncontrado(id);

        var ci = dto.Ci!.Trim();
        // El CI puede repetirse solo si es el del mismo productor.
        if (await db.Productores.AnyAsync(p => p.Ci == ci && p.Id != id, ct))
        {
            ModelState.AddModelError(nameof(dto.Ci), "Ya existe un productor con ese CI.");
            return ValidationProblem(ModelState);
        }

        productor.Nombre = dto.Nombre!.Trim();
        productor.Apellido = dto.Apellido!.Trim();
        productor.Ci = ci;
        productor.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>
    /// Elimina un productor que no tenga predios.
    /// </summary>
    /// <param name="id">Id del productor.</param>
    /// <param name="ct">Token de cancelación.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        var productor = await db.Productores.FindAsync([id], ct);
        if (productor is null)
            return NoEncontrado(id);

        if (await db.Predios.AnyAsync(p => p.ProductorId == id, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "No se puede eliminar el productor",
                Detail = $"El productor con id {id} tiene predios registrados.",
                Status = StatusCodes.Status409Conflict
            });
        }

        db.Productores.Remove(productor);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private NotFoundObjectResult NoEncontrado(int id) =>
        NotFound(new ProblemDetails
        {
            Title = "Productor no encontrado",
            Detail = $"No existe un productor con id {id}.",
            Status = StatusCodes.Status404NotFound
        });
}
