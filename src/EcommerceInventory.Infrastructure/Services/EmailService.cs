using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EcommerceInventory.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly AppSettings _appSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, IOptions<AppSettings> appSettings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.Username, _settings.AppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("Email sent to {Email} — Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} — Subject: {Subject}", toEmail, subject);
        }
    }

    public async Task SendEmailVerificationAsync(string toEmail, string fullName, string verificationToken)
    {
        var verifyUrl = $"{_appSettings.BaseUrl}/api/v1/auth/verify-email?token={Uri.EscapeDataString(verificationToken)}";
        var html = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2 style="color: #2563eb;">Verify Your Email</h2>
                <p>Hello <strong>{fullName}</strong>,</p>
                <p>Please verify your email address:</p>
                <a href="{verifyUrl}" style="background-color: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px;">Verify Email</a>
            </div>
            """;
        await SendAsync(toEmail, fullName, "Verify Your Email Address", html);
    }

    public async Task SendPasswordResetAsync(string toEmail, string fullName, string resetToken)
    {
        var resetUrl = $"{_appSettings.BaseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";
        var html = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <h2 style="color: #dc2626;">Reset Your Password</h2>
                <p>Hello <strong>{fullName}</strong>,</p>
                <a href="{resetUrl}" style="background-color: #dc2626; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px;">Reset Password</a>
            </div>
            """;
        await SendAsync(toEmail, fullName, "Reset Your Password", html);
    }

    public async Task SendLowStockAlertAsync(string toEmail, string productName, int currentQty, int reorderLevel)
    {
        var html = $"""
            <div>
                <h2>⚠️ Low Stock Alert</h2>
                <p>Product: <strong>{productName}</strong></p>
                <p>Current Stock: {currentQty} units</p>
                <p>Reorder Level: {reorderLevel} units</p>
            </div>
            """;
        await SendAsync(toEmail, "Inventory Manager", $"Low Stock Alert: {productName}", html);
    }

    public async Task SendPurchaseOrderStatusAsync(string toEmail, string poNumber, string status)
    {
        var html = $"""
            <div>
                <h2>Purchase Order {status}</h2>
                <p>PO Number: <strong>{poNumber}</strong></p>
                <p>Status: <strong>{status}</strong></p>
            </div>
            """;
        await SendAsync(toEmail, "Team", $"Purchase Order {poNumber} — {status}", html);
    }

    public async Task SendSalesOrderStatusAsync(string toEmail, string soNumber, string status, string customerName)
    {
        var html = $"""
            <div>
                <h2>Sales Order Update</h2>
                <p>Order Number: <strong>{soNumber}</strong></p>
                <p>Status: <strong>{status}</strong></p>
            </div>
            """;
        await SendAsync(toEmail, customerName, $"Order {soNumber} — {status}", html);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string fullName)
    {
        var html = $"""
            <div>
                <h2>Welcome!</h2>
                <p>Hello <strong>{fullName}</strong>,</p>
                <p>Your account has been created successfully.</p>
            </div>
            """;
        await SendAsync(toEmail, fullName, "Welcome to Inventory Management System", html);
    }
}