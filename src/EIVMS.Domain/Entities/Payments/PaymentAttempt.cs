using EIVMS.Domain.Common;
using EIVMS.Domain.Enums.Payments;

namespace EIVMS.Domain.Entities.Payments;

public class PaymentAttempt : BaseEntity
{
    public Guid PaymentId { get; private set; }
    public PaymentStatus FromStatus { get; private set; }
    public PaymentStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public string? GatewayResponse { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public Payment Payment { get; private set; } = null!;

    private PaymentAttempt() { }

    public static PaymentAttempt Create(Guid paymentId, PaymentStatus fromStatus, PaymentStatus toStatus,
        string? reason = null, string? gatewayResponse = null, string? ipAddress = null, string? userAgent = null)
    {
        return new PaymentAttempt
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Reason = reason,
            GatewayResponse = gatewayResponse,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };
    }
}