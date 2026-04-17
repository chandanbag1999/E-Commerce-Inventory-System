using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

public class User : BaseEntity, ISoftDelete
{
    public string FullName             { get; private set; } = string.Empty;
    public string Email                { get; private set; } = string.Empty;
    public string PasswordHash         { get; private set; } = string.Empty;
    public string? Phone               { get; private set; }
    public string? ProfileImageUrl     { get; private set; }
    public string? CloudinaryProfileId { get; private set; }
    public UserStatus Status           { get; private set; } = UserStatus.Active;
    public bool IsEmailVerified        { get; private set; } = false;
    public DateTime? LastLoginAt       { get; private set; }
    public DateTime? DeletedAt         { get; set; }
    public bool IsDeleted              => DeletedAt.HasValue;

    public ICollection<UserRole>             UserRoles             { get; private set; } = new List<UserRole>();
    public ICollection<RefreshToken>         RefreshTokens         { get; private set; } = new List<RefreshToken>();
    public ICollection<PasswordResetToken>   PasswordResetTokens   { get; private set; } = new List<PasswordResetToken>();
    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; private set; } = new List<EmailVerificationToken>();

    public User() { }

    public static User Create(string fullName, string email, string passwordHash, string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        return new User
        {
            FullName     = fullName.Trim(),
            Email        = email.Trim().ToLower(),
            PasswordHash = passwordHash,
            Phone        = phone?.Trim(),
            Status       = UserStatus.Active
        };
    }

    public void UpdateProfile(string fullName, string? phone)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");

        FullName  = fullName.Trim();
        Phone     = phone?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProfileImage(string imageUrl, string cloudinaryId)
    {
        ProfileImageUrl     = imageUrl;
        CloudinaryProfileId = cloudinaryId;
        UpdatedAt           = DateTime.UtcNow;
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        UpdatedAt       = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = newPasswordHash;
        UpdatedAt    = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status    = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status    = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        Status    = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt   = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}