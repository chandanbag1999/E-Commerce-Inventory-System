using System.Security.Claims;
using EIVMS.Application.Modules.Identity.DTOs;
using EIVMS.Application.Modules.Identity.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EIVMS.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        return result.StatusCode switch
        {
            201 => StatusCode(201, result),
            409 => Conflict(result),
            422 => UnprocessableEntity(result),
            _ => BadRequest(result)
        };
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        dto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        dto.UserAgent = Request.Headers["User-Agent"].ToString();

        var result = await _authService.LoginAsync(dto);

        return result.StatusCode switch
        {
            200 => Ok(result),
            423 => StatusCode(423, result),
            _ => Unauthorized(result)
        };
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.LogoutAsync(dto.RefreshToken);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid token" });
        }

        var result = await _authService.GetCurrentUserAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}