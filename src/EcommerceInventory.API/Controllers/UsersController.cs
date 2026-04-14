using EcommerceInventory.API.Authorization;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Users.Commands.ActivateUser;
using EcommerceInventory.Application.Features.Users.Commands.AssignRole;
using EcommerceInventory.Application.Features.Users.Commands.CreateUser;
using EcommerceInventory.Application.Features.Users.Commands.DeactivateUser;
using EcommerceInventory.Application.Features.Users.Commands.DeleteUser;
using EcommerceInventory.Application.Features.Users.Commands.RemoveRole;
using EcommerceInventory.Application.Features.Users.Commands.UpdateUser;
using EcommerceInventory.Application.Features.Users.Commands.UploadProfileImage;
using EcommerceInventory.Application.Features.Users.DTOs;
using EcommerceInventory.Application.Features.Users.Queries.GetAllUsers;
using EcommerceInventory.Application.Features.Users.Queries.GetUserById;
using EcommerceInventory.Application.Features.Users.Queries.SearchUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceInventory.API.Controllers;

/// <summary>
/// Users controller — Full CRUD + Role assignment + Profile image upload
/// 10 endpoints total
/// </summary>
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ─────────────────────────────────────────────
    // POST /api/v1/users  [Admin only]
    // ─────────────────────────────────────────────
    /// <summary>
    /// Create a new user (admin only)
    /// </summary>
    [HasPermission("Users.Create")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        var command = new CreateUserCommand
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Password = dto.Password,
            Phone = dto.Phone
        };

        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Created(string.Empty, new ApiResponse<UserResponseDto>(true, result.Data!, result.Message));
    }

    // ─────────────────────────────────────────────
    // GET /api/v1/users/{id}
    // ─────────────────────────────────────────────
    /// <summary>
    /// Get user by ID
    /// </summary>
    [HasPermission("Users.View")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        var query = new GetUserByIdQuery { UserId = id };
        var result = await _mediator.Send(query, ct);

        if (!result.Success)
        {
            return NotFound(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<UserResponseDto>(true, result.Data!));
    }

    // ─────────────────────────────────────────────
    // GET /api/v1/users
    // ─────────────────────────────────────────────
    /// <summary>
    /// Get all users (paginated, with optional status filter)
    /// </summary>
    [HasPermission("Users.View")]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var query = new GetAllUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Parse status filter if provided
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.UserStatus>(status, true, out var parsedStatus))
        {
            query = query with { Status = parsedStatus };
        }

        var result = await _mediator.Send(query, ct);
        return Ok(new ApiResponse<PagedResult<UserListDto>>(true, result.Data!));
    }

    // ─────────────────────────────────────────────
    // GET /api/v1/users/search
    // ─────────────────────────────────────────────
    /// <summary>
    /// Search users by name or email
    /// </summary>
    [HasPermission("Users.View")]
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] string searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new SearchUsersQuery
        {
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, ct);
        return Ok(new ApiResponse<PagedResult<UserListDto>>(true, result.Data!));
    }

    // ─────────────────────────────────────────────
    // PUT /api/v1/users/{id}
    // ─────────────────────────────────────────────
    /// <summary>
    /// Update user details
    /// </summary>
    [HasPermission("Users.Edit")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto, CancellationToken ct)
    {
        var command = new UpdateUserCommand
        {
            UserId = id,
            FullName = dto.FullName,
            Phone = dto.Phone
        };

        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return result.Message?.Contains("not found") == true
                ? NotFound(new ApiResponse<object>(false, result.Message))
                : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<UserResponseDto>(true, result.Data!, result.Message));
    }

    // ─────────────────────────────────────────────
    // DELETE /api/v1/users/{id}
    // ─────────────────────────────────────────────
    /// <summary>
    /// Soft delete a user
    /// </summary>
    [HasPermission("Users.Delete")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        var command = new DeleteUserCommand { UserId = id };
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return result.Message?.Contains("not found") == true
                ? NotFound(new ApiResponse<object>(false, result.Message))
                : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<object>(true, result.Message ?? "Operation successful"));
    }

    // ─────────────────────────────────────────────
    // POST /api/v1/users/{id}/roles
    // ─────────────────────────────────────────────
    /// <summary>
    /// Assign a role to a user
    /// </summary>
    [HasPermission("Users.AssignRole")]
    [HttpPost("{id:guid}/roles")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleDto dto, CancellationToken ct)
    {
        var command = new AssignRoleCommand
        {
            UserId = id,
            RoleId = dto.RoleId
        };

        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return result.Message?.Contains("not found") == true
                ? NotFound(new ApiResponse<object>(false, result.Message))
                : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<object>(true, result.Message ?? "Operation successful"));
    }

    // ─────────────────────────────────────────────
    // DELETE /api/v1/users/{id}/roles/{roleId}
    // ─────────────────────────────────────────────
    /// <summary>
    /// Remove a role from a user
    /// </summary>
    [HasPermission("Users.AssignRole")]
    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(Guid id, Guid roleId, CancellationToken ct)
    {
        var command = new RemoveRoleCommand
        {
            UserId = id,
            RoleId = roleId
        };

        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return result.Message?.Contains("not found") == true
                ? NotFound(new ApiResponse<object>(false, result.Message))
                : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<object>(true, result.Message ?? "Operation successful"));
    }

    // ─────────────────────────────────────────────
    // POST /api/v1/users/{id}/profile-image
    // ─────────────────────────────────────────────
    /// <summary>
    /// Upload or update user's profile image
    /// </summary>
    [HasPermission("Users.Edit")]
    [HttpPost("{id:guid}/profile-image")]
    [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadProfileImage(Guid id, IFormFile file, CancellationToken ct)
    {
        var command = new UploadProfileImageCommand
        {
            UserId = id,
            File = file
        };

        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<UserResponseDto>(true, result.Data!, result.Message));
    }

    // ─────────────────────────────────────────────
    // PATCH /api/v1/users/{id}/activate
    // ─────────────────────────────────────────────
    /// <summary>
    /// Activate a user account
    /// </summary>
    [HasPermission("Users.Edit")]
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateUser(Guid id, CancellationToken ct)
    {
        var command = new ActivateUserCommand { UserId = id };
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return result.Message?.Contains("not found") == true
                ? NotFound(new ApiResponse<object>(false, result.Message))
                : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<object>(true, result.Message ?? "Operation successful"));
    }

    // ─────────────────────────────────────────────
    // PATCH /api/v1/users/{id}/deactivate
    // ─────────────────────────────────────────────
    /// <summary>
    /// Deactivate a user account
    /// </summary>
    [HasPermission("Users.Edit")]
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(Guid id, CancellationToken ct)
    {
        var command = new DeactivateUserCommand { UserId = id };
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            return result.Message?.Contains("not found") == true
                ? NotFound(new ApiResponse<object>(false, result.Message))
                : BadRequest(new ApiResponse<object>(false, result.Errors.FirstOrDefault() ?? result.Message ?? "Operation failed"));
        }

        return Ok(new ApiResponse<object>(true, result.Message ?? "Operation successful"));
    }
}
