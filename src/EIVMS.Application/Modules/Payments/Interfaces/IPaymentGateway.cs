namespace EIVMS.Application.Modules.Payments.Interfaces;

public interface IPaymentGateway
{
    string ProviderName { get; }
    Task<GatewayOrderResult> CreateOrderAsync(decimal amount, string currency, string orderId, Dictionary<string, string>? metadata = null);
    bool VerifyWebhookSignature(string payload, string signature, string secret);
    Task<GatewayPaymentDetail?> FetchPaymentAsync(string providerPaymentId);
    Task<GatewayRefundResult> InitiateRefundAsync(string providerPaymentId, decimal amount, string? reason = null);
    Task<List<GatewayPaymentDetail>> FetchPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate);
}

public record GatewayOrderResult(
    bool Success,
    string? ProviderOrderId,
    string? PaymentUrl,
    string? ErrorMessage,
    Dictionary<string, string>? Metadata = null
);

public record GatewayPaymentDetail(
    string ProviderPaymentId,
    string Status,
    decimal Amount,
    string Currency,
    string? CustomerEmail,
    string? CustomerPhone,
    DateTime? ProcessedAt,
    Dictionary<string, string>? Metadata = null
);

public record GatewayRefundResult(
    bool Success,
    string? ProviderRefundId,
    string Status,
    string? ErrorMessage
);
