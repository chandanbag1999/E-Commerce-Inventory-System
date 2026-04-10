using EIVMS.Domain.Common;
using EIVMS.Domain.Enums.Payments;

namespace EIVMS.Domain.Entities.Payments;

public class Payment : BaseEntity
{
    public string PaymentNumber { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Created;
    public PaymentProvider Provider { get; private set; } = PaymentProvider.Unknown;
    public string? ProviderPaymentId { get; private set; }
    public string? ProviderRefundId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal RefundedAmount { get; private set; }
    public string Currency { get; private set; } = "INR";
    public string? CustomerEmail { get; private set; }
    public string? CustomerPhone { get; private set; }
    public string? CustomerName { get; private set; }
    public string? BillingAddress { get; private set; }
    public string? Description { get; private set; }
    public string? Notes { get; private set; }
    public string? Metadata { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? PaymentUrl { get; private set; }
    public string? RedirectUrl { get; private set; }
    public string? WebhookPayload { get; private set; }
    public DateTime? WebhookProcessedAt { get; private set; }
    public string? GatewayResponse { get; private set; }

    public ICollection<PaymentAttempt> Attempts { get; private set; } = new List<PaymentAttempt>();
    public ICollection<Refund> Refunds { get; private set; } = new List<Refund>();

    private Payment() { }

    public static Payment Create(Guid orderId, Guid userId, string idempotencyKey, decimal amount,
        string currency = "INR", string? customerEmail = null, string? customerPhone = null,
        string? customerName = null, string? billingAddress = null, string? description = null,
        string? metadata = null, DateTime? expiresAt = null, string? redirectUrl = null)
    {
        if (orderId == Guid.Empty) throw new ArgumentException("Order ID is required");
        if (userId == Guid.Empty) throw new ArgumentException("User ID is required");
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency key is required");
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero");

        return new Payment
        {
            Id = Guid.NewGuid(),
            PaymentNumber = GeneratePaymentNumber(),
            IdempotencyKey = idempotencyKey,
            OrderId = orderId,
            UserId = userId,
            Status = PaymentStatus.Created,
            Amount = amount,
            Currency = currency,
            CustomerEmail = customerEmail,
            CustomerPhone = customerPhone,
            CustomerName = customerName,
            BillingAddress = billingAddress,
            Description = description,
            Metadata = metadata,
            ExpiresAt = expiresAt,
            RedirectUrl = redirectUrl,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static readonly Dictionary<PaymentStatus, List<PaymentStatus>> ValidTransitions = new()
    {
        [PaymentStatus.Created] = new() { PaymentStatus.Pending, PaymentStatus.Failed, PaymentStatus.Expired },
        [PaymentStatus.Pending] = new() { PaymentStatus.Success, PaymentStatus.Failed, PaymentStatus.Expired },
        [PaymentStatus.Success] = new() { PaymentStatus.RefundInitiated, PaymentStatus.Disputed },
        [PaymentStatus.Failed] = new() { PaymentStatus.Pending },
        [PaymentStatus.RefundInitiated] = new() { PaymentStatus.Refunded, PaymentStatus.PartiallyRefunded, PaymentStatus.Success },
        [PaymentStatus.Refunded] = new(),
        [PaymentStatus.PartiallyRefunded] = new() { PaymentStatus.RefundInitiated },
        [PaymentStatus.Expired] = new() { PaymentStatus.Pending },
        [PaymentStatus.Disputed] = new() { PaymentStatus.RefundInitiated, PaymentStatus.Success }
    };

    private void TransitionTo(PaymentStatus newStatus, string? reason = null, string? gatewayResponse = null)
    {
        if (!ValidTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidOperationException($"Invalid payment state transition: {Status} → {newStatus}");

        var previousStatus = Status;
        Status = newStatus;
        Attempts.Add(PaymentAttempt.Create(Id, previousStatus, newStatus, reason, gatewayResponse));
        SetUpdatedAt();
    }

    public void InitiateWithProvider(PaymentProvider provider, string? providerPaymentId, string? paymentUrl = null,
        string? gatewayResponse = null)
    {
        if (Status != PaymentStatus.Created)
            throw new InvalidOperationException("Payment must be in Created status to be initiated");

        Provider = provider;
        ProviderPaymentId = providerPaymentId;
        PaymentUrl = paymentUrl;
        GatewayResponse = gatewayResponse;
        TransitionTo(PaymentStatus.Pending, "Payment initiated with provider", gatewayResponse);
    }

    public void MarkAsSuccess(string? providerPaymentId = null, string? gatewayResponse = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Payment must be in Pending status to be marked as success");

        if (!string.IsNullOrEmpty(providerPaymentId))
            ProviderPaymentId = providerPaymentId;

        GatewayResponse = gatewayResponse;
        ProcessedAt = DateTime.UtcNow;
        TransitionTo(PaymentStatus.Success, "Payment processed successfully", gatewayResponse);
    }

    public void MarkAsFailed(string? failureReason = null, string? gatewayResponse = null)
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Created)
            throw new InvalidOperationException("Payment must be in Created or Pending status to be marked as failed");

        FailureReason = failureReason;
        GatewayResponse = gatewayResponse;
        TransitionTo(PaymentStatus.Failed, failureReason, gatewayResponse);
    }

    public void MarkAsExpired()
    {
        if (Status is not (PaymentStatus.Created or PaymentStatus.Pending))
            throw new InvalidOperationException("Payment must be in Created or Pending status to expire");

        TransitionTo(PaymentStatus.Expired, "Payment expired");
    }

    public void InitiateRefund(decimal amount, string? reason = null, string? notes = null)
    {
        if (Status != PaymentStatus.Success && Status != PaymentStatus.PartiallyRefunded)
            throw new InvalidOperationException("Payment must be in Success or PartiallyRefunded status to initiate refund");

        var refundableAmount = Amount - RefundedAmount;
        if (amount > refundableAmount)
            throw new ArgumentException($"Refund amount exceeds refundable amount: {refundableAmount}");

        var refund = Refund.Create(Id, amount, reason, notes, isFullRefund: amount == refundableAmount);
        Refunds.Add(refund);

        if (amount == refundableAmount)
            TransitionTo(PaymentStatus.RefundInitiated, reason);
        else
            TransitionTo(PaymentStatus.RefundInitiated, $"Partial refund initiated: {amount}");
    }

    public void MarkRefundAsSuccess(string? providerRefundId = null)
    {
        var pendingRefund = Refunds.FirstOrDefault(r => r.Status == RefundStatus.Processing);
        if (pendingRefund == null)
            throw new InvalidOperationException("No pending refund to process");

        if (!string.IsNullOrEmpty(providerRefundId))
            ProviderRefundId = providerRefundId;

        pendingRefund.MarkAsSuccess(providerRefundId);
        RefundedAmount += pendingRefund.Amount;

        if (RefundedAmount >= Amount)
        {
            Status = PaymentStatus.Refunded;
            Attempts.Add(PaymentAttempt.Create(Id, PaymentStatus.RefundInitiated, PaymentStatus.Refunded,
                "Full refund processed"));
        }
        else
        {
            Status = PaymentStatus.PartiallyRefunded;
            Attempts.Add(PaymentAttempt.Create(Id, PaymentStatus.RefundInitiated, PaymentStatus.PartiallyRefunded,
                $"Partial refund processed: {pendingRefund.Amount}"));
        }
        SetUpdatedAt();
    }

    public void MarkRefundAsFailed(string? reason = null)
    {
        var pendingRefund = Refunds.FirstOrDefault(r => r.Status == RefundStatus.Processing);
        if (pendingRefund == null)
            throw new InvalidOperationException("No pending refund to fail");

        pendingRefund.MarkAsFailed(reason);

        if (RefundedAmount == 0)
            Status = PaymentStatus.Success;
        else
            Status = PaymentStatus.PartiallyRefunded;

        Attempts.Add(PaymentAttempt.Create(Id, PaymentStatus.RefundInitiated, Status, $"Refund failed: {reason}"));
        SetUpdatedAt();
    }

    public void MarkAsDisputed(string? reason = null)
    {
        if (Status != PaymentStatus.Success)
            throw new InvalidOperationException("Only successful payments can be disputed");

        Notes = reason;
        TransitionTo(PaymentStatus.Disputed, reason);
    }

    public void ResolveDispute(bool favorCustomer, string? notes = null)
    {
        if (Status != PaymentStatus.Disputed)
            throw new InvalidOperationException("Payment must be in Disputed status to be resolved");

        Notes = notes;
        TransitionTo(favorCustomer ? PaymentStatus.RefundInitiated : PaymentStatus.Success,
            $"Dispute resolved, {(favorCustomer ? "refund initiated" : "payment confirmed")}");
    }

    public void ProcessWebhook(string payload)
    {
        WebhookPayload = payload;
        WebhookProcessedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void UpdateRedirectUrl(string redirectUrl)
    {
        RedirectUrl = redirectUrl;
        SetUpdatedAt();
    }

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value && Status is PaymentStatus.Created or PaymentStatus.Pending;
    public bool CanBeRefunded => Status == PaymentStatus.Success || Status == PaymentStatus.PartiallyRefunded;
    public bool IsTerminal => Status is PaymentStatus.Refunded or PaymentStatus.Failed or PaymentStatus.Expired;
    public decimal RefundableAmount => Amount - RefundedAmount;

    private static string GeneratePaymentNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(100000, 999999);
        return $"PAY-{timestamp}-{random}";
    }
}