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
using EcommerceInventory.Application.Features.Auth.Queries.GetMe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

public class AuthController : BaseApiController
{
    private readonly ICurrentUserService _currentUser;

    public AuthController(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<object>.Ok(result,
            "Registration successful. " +
            "Please check your email to verify your account."));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(result, "Login successful."));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(result,
            "Token refreshed successfully."));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequestDto request,
        CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Not authenticated.");

        var command = new LogoutCommand
        {
            RefreshToken = request.RefreshToken,
            UserId       = userId
        };

        await Mediator.Send(command, ct);
        return Ok(ApiResponse.Ok("Logged out successfully."));
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailCommand command,
        CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return Ok(ApiResponse.Ok(
            "Email verified successfully. You can now login."));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return Ok(ApiResponse.Ok(
            "If an account exists with this email, " +
            "a password reset link has been sent."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return Ok(ApiResponse.Ok(
            "Password reset successful. Please login with your new password."));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Not authenticated.");

        var result = await Mediator.Send(
            new GetMeQuery { UserId = userId }, ct);

        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto request,
        CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Not authenticated.");

        var command = new ChangePasswordCommand
        {
            UserId      = userId,
            OldPassword = request.OldPassword,
            NewPassword = request.NewPassword
        };

        await Mediator.Send(command, ct);
        return Ok(ApiResponse.Ok(
            "Password changed successfully."));
    }
}

public record LogoutRequestDto(string RefreshToken);
public record ChangePasswordRequestDto(string OldPassword, string NewPassword);