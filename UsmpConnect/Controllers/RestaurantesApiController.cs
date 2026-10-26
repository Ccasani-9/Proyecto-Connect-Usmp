using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsmpConnect.Models;
using UsmpConnect.Services;
using UsmpConnect.ViewModels;

namespace UsmpConnect.Controllers;

[ApiController]
[IgnoreAntiforgeryToken]
[Produces("application/json")]
public class RestaurantesApiController(CatalogoRestaurantesService catalogo) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("/api/restaurantes")]
    [ProducesResponseType<IEnumerable<RestauranteItemVM>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RestauranteItemVM>>> GetRestaurantes(
        [FromQuery] CategoriaRestaurante? categoria,
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var (restaurantes, _) = await catalogo.FiltrarAsync(categoria, q, cancellationToken);
        return Ok(restaurantes);
    }

    [AllowAnonymous]
    [HttpGet("/api/restaurantes/{id:int}")]
    [ProducesResponseType<RestauranteDetalleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RestauranteDetalleDto>> GetRestaurante(
        int id,
        CancellationToken cancellationToken)
    {
        var detalle = await catalogo.ObtenerDetalleAsync(id, cancellationToken);
        return detalle is null ? NotFound() : Ok(detalle);
    }

    [Authorize]
    [HttpPost("/api/restaurantes/{id:int}/resenas")]
    [ProducesResponseType<ResenaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResenaDto>> CrearResena(
        int id,
        [FromBody] GuardarResenaRequest request,
        CancellationToken cancellationToken)
    {
        var creada = await catalogo.CrearResenaAsync(
            id,
            User.GetUserId(),
            User.GetNombre(),
            request.Puntuacion,
            request.Comentario,
            cancellationToken);

        if (creada is null)
            return NotFound();

        return CreatedAtAction(nameof(GetRestaurante), new { id }, creada);
    }

    [Authorize]
    [HttpPut("/api/resenas/{id:int}")]
    [ProducesResponseType<ResenaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResenaDto>> EditarResena(
        int id,
        [FromBody] GuardarResenaRequest request,
        CancellationToken cancellationToken)
    {
        var (estado, resena) = await catalogo.EditarResenaPropiaAsync(
            id,
            User.GetUserId(),
            User.GetNombre(),
            request.Puntuacion,
            request.Comentario,
            cancellationToken);

        return estado switch
        {
            EstadoOperacionResena.NoEncontrada => NotFound(),
            EstadoOperacionResena.Prohibida => Forbid(),
            _ => Ok(resena)
        };
    }

    [Authorize]
    [HttpDelete("/api/resenas/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarResena(
        int id,
        CancellationToken cancellationToken)
    {
        var estado = await catalogo.EliminarResenaPropiaAsync(id, User.GetUserId(), cancellationToken);

        return estado switch
        {
            EstadoOperacionResena.NoEncontrada => NotFound(),
            EstadoOperacionResena.Prohibida => Forbid(),
            _ => NoContent()
        };
    }
}
