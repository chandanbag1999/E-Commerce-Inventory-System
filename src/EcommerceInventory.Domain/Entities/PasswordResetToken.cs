using EcommerceInventory.Domain.Common;

namespace EcommerceInventory.Domain.Entities;


// Password reset token for secure password recovery
public class PasswordResetToken : BaseEntity
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
