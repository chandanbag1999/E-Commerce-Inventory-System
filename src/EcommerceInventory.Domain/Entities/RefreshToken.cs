using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid     UserId      { get; set; }
    public string   Token       { get; set; } = string.Empty;
    public DateTime ExpiresAt   { get; set; }
    public bool     IsRevoked   { get; set; } = false;
    public DateTime? RevokedAt  { get; set; }
    public string?  ReplacedBy  { get; set; }
    public string?  DeviceInfo  { get; set; }

    public User User { get; set; } = null!;

    protected RefreshToken() { }

    public static RefreshToken Create(Guid userId, string token,
                                      DateTime expiresAt, string? deviceInfo = null)
    {
        return new RefreshToken
        {
            UserId     = userId,
            Token      = token,
            ExpiresAt  = expiresAt,
            DeviceInfo = deviceInfo
        };
    }

    public void Revoke(string? replacedBy = null)
    {
        IsRevoked   = true;
        RevokedAt   = DateTime.UtcNow;
        ReplacedBy  = replacedBy;
    }

    public bool IsExpired  => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive   => !IsRevoked && !IsExpired;
}