using EIVMS.Domain.Common;

namespace EIVMS.Domain.Entities.Identity;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public string TokenFamily { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; } = false;
    public string? RevokedReason { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;

    public User User { get; private set; } = null!;

    private RefreshToken() { }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        string tokenFamily,
        DateTime expiresAt,
        string ipAddress,
        string userAgent)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = tokenHash,
            TokenFamily = tokenFamily,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Revoke(string reason, string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedReason = reason;
        ReplacedByToken = replacedByToken;
        SetUpdatedAt();
    }

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
}