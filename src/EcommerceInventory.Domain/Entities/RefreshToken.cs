using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

// Entity representing a refresh token for user authentication
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedBy { get; set; }
    public string? DeviceInfo { get; set; }

    // Navigation property
    public User User { get; set; } = null!;

    // Factory method to create a new RefreshToken with validation
    public static RefreshToken Create(
        Guid userId,
        string token,
        DateTime expiresAt,
        string? deviceInfo = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException("Token cannot be empty");

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            DeviceInfo = deviceInfo,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Method to revoke the refresh token
    public void Revoke(string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedBy = replacedByToken;
    }
}
