namespace EIVMS.Application.Modules.Payments.DTOs;

public record WebhookPayloadDto(
    string Event,
    string PaymentId,
    string OrderId,
    string Status,
    string Signature,
    string RawBody
);