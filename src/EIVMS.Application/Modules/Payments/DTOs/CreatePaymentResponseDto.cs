namespace EIVMS.Application.Modules.Payments.DTOs;

public record CreatePaymentResponseDto(
    Guid PaymentId,
    string ProviderOrderId,
    string? CheckoutUrl,
    decimal Amount,
    string Currency,
    string Status
);