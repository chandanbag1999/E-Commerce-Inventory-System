using EIVMS.Domain.Enums;

namespace EIVMS.Application.Modules.UserManagement.DTOs.User;

public class UserProfileResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsPhoneVerified { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public string? DisplayName { get; set; }
    public UserStatus Status { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public NotificationPreference NotificationPreferences { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public bool IsEmailVerified { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<UserOrganizationDto> Organizations { get; set; } = new();
}

public class UserOrganizationDto
{
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public bool IsAdmin { get; set; }
}