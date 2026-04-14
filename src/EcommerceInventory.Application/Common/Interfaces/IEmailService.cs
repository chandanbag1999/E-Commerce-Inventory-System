namespace EcommerceInventory.Application.Common.Interfaces;

/// <summary>
/// Service for sending emails via Gmail SMTP
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
    Task SendEmailVerificationAsync(string toEmail, string verificationLink);
    Task SendPasswordResetAsync(string toEmail, string resetLink);
    Task SendLowStockAlertAsync(string productName, int currentQty, int reorderLevel);
}
