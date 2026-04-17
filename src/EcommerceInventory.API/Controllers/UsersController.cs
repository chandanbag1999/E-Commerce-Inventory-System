using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Users.Commands.ActivateUser;
using EcommerceInventory.Application.Features.Users.Commands.AssignRole;
using EcommerceInventory.Application.Features.Users.Commands.CreateUser;
using EcommerceInventory.Application.Features.Users.Commands.DeactivateUser;
using EcommerceInventory.Application.Features.Users.Commands.DeleteUser;
using EcommerceInventory.Application.Features.Users.Commands.RevokeRole;
using EcommerceInventory.Application.Features.Users.Commands.UpdateUser;
using EcommerceInventory.Application.Features.Users.Commands.UploadProfileImage;
using EcommerceInventory.Application.Features.Users.Queries.GetAllUsers;
using EcommerceInventory.Application.Features.Users.Queries.GetUserById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly ICurrentUserService _currentUser;

    public UsersController(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    // GET /api/v1/users
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllUsersQuery query,
        CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // GET /api/v1/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetUserByIdQuery { Id = id }, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    // POST /api/v1/users
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<object>.Ok(result,
            "User created successfully."));
    }

    // PUT /api/v1/users/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserCommand command,
        CancellationToken ct)
    {
        command.Id = id;
        var result = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(result,
            "User updated successfully."));
    }

    // DELETE /api/v1/users/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteUserCommand { Id = id }, ct);
        return Ok(ApiResponse.Ok("User deleted successfully."));
    }

    // PATCH /api/v1/users/{id}/activate
    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new ActivateUserCommand { Id = id }, ct);
        return Ok(ApiResponse.Ok("User activated successfully."));
    }

    // PATCH /api/v1/users/{id}/deactivate
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeactivateUserCommand { Id = id }, ct);
        return Ok(ApiResponse.Ok("User deactivated successfully."));
    }

    // POST /api/v1/users/{id}/profile-image
    [HttpPost("{id:guid}/profile-image")]
    public async Task<IActionResult> UploadProfileImage(
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("File is required."));

        await using var stream = file.OpenReadStream();

        var command = new UploadProfileImageCommand
        {
            UserId      = id,
            FileStream  = stream,
            FileName    = file.FileName,
            ContentType = file.ContentType
        };

        var imageUrl = await Mediator.Send(command, ct);
        return Ok(ApiResponse<object>.Ok(
            new { imageUrl }, "Profile image uploaded successfully."));
    }

    // POST /api/v1/users/{id}/assign-role
    [HttpPost("{id:guid}/assign-role")]
    public async Task<IActionResult> AssignRole(
        Guid id,
        [FromBody] AssignRoleRequestDto request,
        CancellationToken ct)
    {
        var command = new AssignRoleCommand
        {
            UserId = id,
            RoleId = request.RoleId
        };
        await Mediator.Send(command, ct);
        return Ok(ApiResponse.Ok("Role assigned successfully."));
    }

    // DELETE /api/v1/users/{id}/revoke-role/{roleId}
    [HttpDelete("{id:guid}/revoke-role/{roleId:guid}")]
    public async Task<IActionResult> RevokeRole(
        Guid id, Guid roleId, CancellationToken ct)
    {
        var command = new RevokeRoleCommand
        {
            UserId = id,
            RoleId = roleId
        };
        await Mediator.Send(command, ct);
        return Ok(ApiResponse.Ok("Role revoked successfully."));
    }
}

public record AssignRoleRequestDto(Guid RoleId);
