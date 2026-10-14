using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TableX_API.DTOs;
using TableX_API.Lib.Consts;
using TableX_API.Models;
using TableX_API.Services;

namespace TableX_API.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    // GET api/users/all
    [Authorize(Roles = Roles.Caja)]
    [HttpGet("all")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _userService.GetAllAsync(ct));
    }

    // GET api/users/5
    [Authorize(Roles = Roles.Caja)]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken ct)
    {
        return Ok(await _userService.GetByIdAsync(id, ct));
    }

    // POST api/users/register
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var user = await _userService.CreateAsync(request, User, ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }
}