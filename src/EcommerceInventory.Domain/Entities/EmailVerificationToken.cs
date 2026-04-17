using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Domain.Entities;

public class EmailVerificationToken : BaseEntity
{
    public Guid     UserId    { get; set; }
    public string   TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool     IsUsed    { get; set; } = false;

    public User User { get; set; } = null!;

    protected EmailVerificationToken() { }

    public static EmailVerificationToken Create(Guid userId, string tokenHash, DateTime expiresAt)
    {
        return new EmailVerificationToken
        {
            UserId    = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt
        };
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public void MarkUsed()
    {
        IsUsed = true;
    }
}