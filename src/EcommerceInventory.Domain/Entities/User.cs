using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

/// <summary>
/// User entity - represents a system user
/// </summary>
public class User : AuditableEntity, ISoftDelete
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? CloudinaryProfileId { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool IsEmailVerified { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Factory method to create a new user
    /// </summary>
    public static User Create(
        string fullName,
        string email,
        string passwordHash,
        string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty");

        return new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            Email = email.Trim().ToLower(),
            PasswordHash = passwordHash,
            Phone = phone?.Trim(),
            Status = UserStatus.Active,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Records a login by updating last_login_at
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets profile image information
    /// </summary>
    public void SetProfileImage(string imageUrl, string cloudinaryId)
    {
        ProfileImageUrl = imageUrl;
        CloudinaryProfileId = cloudinaryId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the user account
    /// </summary>
    public void Activate()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user account
    /// </summary>
    public void Deactivate()
    {
        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft deletes the user
    /// </summary>
    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates password hash
    /// </summary>
    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }
}
