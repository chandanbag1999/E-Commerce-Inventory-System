using Microsoft.AspNetCore.Http;

namespace EcommerceInventory.Application.Features.Users.DTOs;

/// <summary>
/// DTO for creating a new user
/// </summary>
public record CreateUserDto(
    string FullName,
    string Email,
    string Password,
    string? Phone = null
);

/// <summary>
/// DTO for updating an existing user
/// </summary>
public record UpdateUserDto(
    string FullName,
    string? Phone = null
);

/// <summary>
/// DTO for user response with full details
/// </summary>
public class UserResponseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for user list view (lightweight)
/// </summary>
public class UserListDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for assigning a role to a user
/// </summary>
public record AssignRoleDto(
    Guid RoleId
);

/// <summary>
/// DTO for uploading/ updating a user's profile image
/// </summary>
public class ProfileImageDto
{
    public IFormFile? File { get; set; }
}

/// <summary>
/// DTO for user status update
/// </summary>
public record UpdateUserStatusDto(
    string Status
);
