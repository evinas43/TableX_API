using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TableX_API.DTOs;
using TableX_API.Lib.Consts;
using TableX_API.Models;
using TableX_API.Services;

namespace TableX_API.Controllers;

[ApiController]
[Route("api/mesas")]
[Authorize]
[Produces("application/json")]
public class MesaController : ControllerBase
{
    private readonly MesaService _mesaService;

    public MesaController(MesaService mesaService)
    {
        _mesaService = mesaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<MesaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MesaDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _mesaService.GetAllAsync(ct));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MesaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MesaDto>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _mesaService.GetByIdAsync(id, ct));
    }

    [Authorize(Roles = Roles.Caja)]
    [HttpPost]
    [ProducesResponseType(typeof(MesaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MesaDto>> Create([FromBody] MesaRequest request, CancellationToken ct)
    {
        var mesa = await _mesaService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = mesa.Id }, mesa);
    }

    [Authorize(Roles = Roles.Caja)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MesaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MesaDto>> Update(int id, [FromBody] MesaRequest request, CancellationToken ct)
    {
        return Ok(await _mesaService.UpdateAsync(id, request, ct));
    }

    [Authorize(Roles = Roles.Caja)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mesaService.DeleteAsync(id, ct);
        return NoContent();
    }
}
