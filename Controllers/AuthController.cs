using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TableX_API.DTOs;
using TableX_API.Extensions;
using TableX_API.Models;
using TableX_API.Services;

namespace TableX_API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly UserService _userService;

    public AuthController(AuthService authService, UserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    // POST api/auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        return Ok(await _authService.LoginAsync(request, ct));
    }

    // GET api/auth/Me
    [Authorize]
    [HttpGet("Me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken ct)
    {
        return Ok(await _userService.GetByIdAsync(User.GetUserId(), ct));
    }
}
