namespace EcommerceInventory.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string fullName, string verificationToken);
    Task SendPasswordResetAsync(string toEmail, string fullName, string resetToken);
    Task SendLowStockAlertAsync(string toEmail, string productName, int currentQty, int reorderLevel);
    Task SendPurchaseOrderStatusAsync(string toEmail, string poNumber, string status);
    Task SendSalesOrderStatusAsync(string toEmail, string soNumber, string status, string customerName);
    Task SendWelcomeEmailAsync(string toEmail, string fullName);
}
