using EIVMS.Domain.Common;
using EIVMS.Domain.Enums.Payments;

namespace EIVMS.Domain.Entities.Payments;

public class Refund : BaseEntity
{
    public string RefundNumber { get; private set; } = string.Empty;
    public Guid PaymentId { get; private set; }
    public Guid? RequestedByUserId { get; private set; }
    public decimal Amount { get; private set; }
    public RefundStatus Status { get; private set; } = RefundStatus.Pending;
    public string? Reason { get; private set; }
    public string? Notes { get; private set; }
    public string? ProviderRefundId { get; private set; }
    public string? GatewayResponse { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public bool IsFullRefund { get; private set; }

    public Payment Payment { get; private set; } = null!;

    private Refund() { }

    public static Refund Create(Guid paymentId, decimal amount, string? reason = null, string? notes = null,
        Guid? requestedByUserId = null, bool isFullRefund = false)
    {
        if (paymentId == Guid.Empty) throw new ArgumentException("Payment ID is required");
        if (amount <= 0) throw new ArgumentException("Refund amount must be greater than zero");

        return new Refund
        {
            Id = Guid.NewGuid(),
            RefundNumber = GenerateRefundNumber(),
            PaymentId = paymentId,
            Amount = amount,
            Reason = reason,
            Notes = notes,
            RequestedByUserId = requestedByUserId,
            IsFullRefund = isFullRefund,
            Status = RefundStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void InitiateWithProvider(string? providerRefundId = null, string? gatewayResponse = null)
    {
        if (Status != RefundStatus.Pending)
            throw new InvalidOperationException("Refund must be in Pending status to be initiated");

        ProviderRefundId = providerRefundId;
        GatewayResponse = gatewayResponse;
        Status = RefundStatus.Processing;
        SetUpdatedAt();
    }

    public void MarkAsSuccess(string? providerRefundId = null, string? gatewayResponse = null)
    {
        if (Status != RefundStatus.Processing)
            throw new InvalidOperationException("Refund must be in Processing status to be marked as success");

        if (!string.IsNullOrEmpty(providerRefundId))
            ProviderRefundId = providerRefundId;

        GatewayResponse = gatewayResponse;
        ProcessedAt = DateTime.UtcNow;
        Status = RefundStatus.Succeeded;
        SetUpdatedAt();
    }

    public void MarkAsFailed(string? failureReason = null, string? gatewayResponse = null)
    {
        if (Status != RefundStatus.Processing)
            throw new InvalidOperationException("Refund must be in Processing status to be marked as failed");

        FailureReason = failureReason;
        GatewayResponse = gatewayResponse;
        Status = RefundStatus.Failed;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        if (Status != RefundStatus.Pending)
            throw new InvalidOperationException("Only pending refunds can be cancelled");

        Status = RefundStatus.Cancelled;
        SetUpdatedAt();
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
        SetUpdatedAt();
    }

    public bool IsTerminal => Status is RefundStatus.Succeeded or RefundStatus.Failed or RefundStatus.Cancelled;

    private static string GenerateRefundNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(100000, 999999);
        return $"REF-{timestamp}-{random}";
    }
}