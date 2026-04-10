using EIVMS.Domain.Common;
using EIVMS.Domain.Enums;

namespace EIVMS.Domain.Entities.Identity;

public class User : BaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public bool IsEmailVerified { get; private set; } = false;
    public int FailedLoginAttempts { get; private set; } = 0;
    public DateTime? LockedUntil { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public UserManagement.UserProfile? UserProfile { get; private set; }
    public ICollection<UserManagement.UserAuditLog> AuditLogs { get; private set; } = new List<UserManagement.UserAuditLog>();

    private User() { }

    public static User Create(string firstName, string lastName, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsEmailVerified = false,
            FailedLoginAttempts = 0
        };
    }

    public void IncrementFailedAttempts()
    {
        FailedLoginAttempts++;
        SetUpdatedAt();
    }

    public void LockAccount(DateTime until)
    {
        LockedUntil = until;
        SetUpdatedAt();
    }

    public void ResetFailedAttempts()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
        SetUpdatedAt();
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        SetUpdatedAt();
    }

    public bool IsLockedOut()
    {
        return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public void Suspend()
    {
        Status = UserStatus.Suspended;
        SetUpdatedAt();
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        IsActive = true;
        SetUpdatedAt();
    }

    public void SoftDelete()
    {
        Status = UserStatus.Deleted;
        IsActive = false;
        SetUpdatedAt();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty");
        PasswordHash = newPasswordHash;
        SetUpdatedAt();
    }

    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        SetUpdatedAt();
    }
}