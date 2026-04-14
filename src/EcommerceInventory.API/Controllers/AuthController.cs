using EcommerceInventory.API.Controllers;
using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Auth.Commands.ChangePassword;
using EcommerceInventory.Application.Features.Auth.Commands.ForgotPassword;
using EcommerceInventory.Application.Features.Auth.Commands.Login;
using EcommerceInventory.Application.Features.Auth.Commands.Logout;
using EcommerceInventory.Application.Features.Auth.Commands.RefreshToken;
using EcommerceInventory.Application.Features.Auth.Commands.Register;
using EcommerceInventory.Application.Features.Auth.Commands.ResetPassword;
using EcommerceInventory.Application.Features.Auth.Commands.VerifyEmail;
using EcommerceInventory.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

/// <summary>
/// Authentication controller — Register, Login, Refresh, Logout, etc.
/// 9 endpoints total
/// </summary>
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    // ─────────────────────────────────────────────
    // POST /auth/register
    // ─────────────────────────────────────────────
    /// <summary>
    /// Register a new user account
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return BadRequest(result.Errors.FirstOrDefault() ?? result.Message ?? "Registration failed");
        }

        return Created(string.Empty, new ApiResponse<RegisterResponseDto>(true, result.Data!, result.Message));
    }

    // ─────────────────────────────────────────────
    // POST /auth/login
    // ─────────────────────────────────────────────
    /// <summary>
    /// Login and receive access + refresh token pair
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return Unauthorized(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<LoginResponseDto>(true, result.Data!, result.Message));
    }

    // ─────────────────────────────────────────────
    // POST /auth/refresh-token
    // ─────────────────────────────────────────────
    /// <summary>
    /// Rotate tokens — send expired access token + valid refresh token, get new pair
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return Unauthorized(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<LoginResponseDto>(true, result.Data!, result.Message));
    }

    // ─────────────────────────────────────────────
    // POST /auth/logout  [Bearer Auth Required]
    // ─────────────────────────────────────────────
    /// <summary>
    /// Logout — revoke the refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request, CancellationToken ct)
    {
        var command = new LogoutCommand
        {
            RefreshToken = request.RefreshToken,
            UserId = GetCurrentUserId()
        };

        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<object>(true, result.Message ?? "Operation successful"));
    }

    // ─────────────────────────────────────────────
    // POST /auth/verify-email
    // ─────────────────────────────────────────────
    /// <summary>
    /// Verify email using the token sent via email
    /// </summary>
    [AllowAnonymous]
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<object>(true, result.Message ?? "Operation successful"));
    }

    // ─────────────────────────────────────────────
    // POST /auth/forgot-password
    // ─────────────────────────────────────────────
    /// <summary>
    /// Request a password reset email (always returns success for security)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<object>(true, result.Message ?? "Operation successful"));
    }

    // ─────────────────────────────────────────────
    // POST /auth/reset-password
    // ─────────────────────────────────────────────
    /// <summary>
    /// Reset password using the token from email
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<object>(true, result.Message ?? "Operation successful"));
    }

    // ─────────────────────────────────────────────
    // GET /auth/me  [Bearer Auth Required]
    // ─────────────────────────────────────────────
    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == null)
        {
            return Unauthorized(new ApiResponse<object>(false, "Not authenticated"));
        }

        var user = new CurrentUserInfoDto
        {
            Id = _currentUser.UserId.Value,
            Email = _currentUser.Email ?? string.Empty,
            Roles = _currentUser.Roles,
            Permissions = _currentUser.Permissions
        };

        return Ok(new ApiResponse<CurrentUserInfoDto>(true, user));
    }

    // ─────────────────────────────────────────────
    // POST /auth/change-password  [Bearer Auth Required]
    // ─────────────────────────────────────────────
    /// <summary>
    /// Change current user's password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken ct)
    {
        // Ensure user can only change their own password
        command.UserId = GetCurrentUserId();

        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<object>(true, result.Message ?? "Operation successful"));
    }
}

// ─────────────────────────────────────────────
// DTOs for Auth Controller requests/responses
// ─────────────────────────────────────────────

public class LogoutRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class CurrentUserInfoDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
    public IList<string> Permissions { get; set; } = new List<string>();
}
