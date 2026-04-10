namespace EIVMS.Application.Modules.Payments.DTOs;

public record RefundResponseDto(
    Guid RefundId,
    Guid PaymentId,
    decimal Amount,
    string Status,
    string? ProviderRefundId,
    DateTime CreatedAt
);