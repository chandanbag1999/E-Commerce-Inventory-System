using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Domain.Entities;

// Email verification token for account activation
public class EmailVerificationToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;

    // Navigation property
    public User User { get; set; } = null!;

    public void MarkAsUsed()
    {
        IsUsed = true;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiresAt;
    }
}
