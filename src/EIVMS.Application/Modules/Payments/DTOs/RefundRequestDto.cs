namespace EIVMS.Application.Modules.Payments.DTOs;

public record RefundRequestDto(
    Guid PaymentId,
    decimal? Amount,
    string Reason,
    string InitiatedBy = "admin"
);