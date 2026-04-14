using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EcommerceInventory.Infrastructure.Services;

/// <summary>
/// Email service using MailKit for Gmail SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            if (isHtml)
            {
                bodyBuilder.HtmlBody = body;
            }
            else
            {
                bodyBuilder.TextBody = body;
            }

            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            // Don't throw - email failures shouldn't break the main flow
        }
    }

    public async Task SendEmailVerificationAsync(string toEmail, string verificationLink)
    {
        var subject = "Verify Your Email - Ecommerce Inventory System";
        var body = $@"
            <h2>Email Verification</h2>
            <p>Thank you for registering with Ecommerce Inventory System.</p>
            <p>Please click the link below to verify your email address:</p>
            <p><a href='{verificationLink}'>Verify Email</a></p>
            <p>If you didn't create an account, please ignore this email.</p>
            <br/>
            <p>Best regards,<br/>Ecommerce Inventory Team</p>
        ";

        await SendEmailAsync(toEmail, subject, body, true);
    }

    public async Task SendPasswordResetAsync(string toEmail, string resetLink)
    {
        var subject = "Password Reset - Ecommerce Inventory System";
        var body = $@"
            <h2>Password Reset Request</h2>
            <p>You requested to reset your password.</p>
            <p>Please click the link below to set a new password:</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>This link will expire in 1 hour.</p>
            <p>If you didn't request a password reset, please ignore this email.</p>
            <br/>
            <p>Best regards,<br/>Ecommerce Inventory Team</p>
        ";

        await SendEmailAsync(toEmail, subject, body, true);
    }

    public async Task SendLowStockAlertAsync(string productName, int currentQty, int reorderLevel)
    {
        var subject = $"⚠️ Low Stock Alert - {productName}";
        var body = $@"
            <h2>Low Stock Alert</h2>
            <p>The following product is running low on stock:</p>
            <ul>
                <li><strong>Product:</strong> {productName}</li>
                <li><strong>Current Quantity:</strong> {currentQty}</li>
                <li><strong>Reorder Level:</strong> {reorderLevel}</li>
            </ul>
            <p>Please consider restocking this item soon.</p>
            <br/>
            <p>Best regards,<br/>Ecommerce Inventory System</p>
        ";

        // Send to a predefined admin email (from config)
        var adminEmail = _emailSettings.SenderEmail; // In production, use a separate admin email config
        await SendEmailAsync(adminEmail, subject, body, true);
    }
}
