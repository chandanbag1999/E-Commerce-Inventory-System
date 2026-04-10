using EIVMS.Domain.Common;
using EIVMS.Domain.Enums.Orders;

namespace EIVMS.Domain.Entities.Orders;

public class OrderReturnItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public ReturnReason Reason { get; private set; }
    public int Quantity { get; private set; }
    public decimal RefundAmount { get; private set; }
    public string? Notes { get; private set; }
    public string Status { get; private set; } = "Pending";
    public string? AdminNotes { get; private set; }
    public Guid? ProcessedByUserId { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ProofImageUrl { get; private set; }
    public Order Order { get; private set; } = null!;

    private OrderReturnItem() { }

    public static OrderReturnItem Create(Guid orderId, Guid orderItemId, ReturnReason reason,
        int quantity, decimal refundAmount, string? notes = null, string? proofImageUrl = null)
    {
        return new OrderReturnItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            OrderItemId = orderItemId,
            Reason = reason,
            Quantity = quantity,
            RefundAmount = refundAmount,
            Notes = notes,
            ProofImageUrl = proofImageUrl,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve(Guid processedBy, string? adminNotes = null)
    {
        Status = "Approved";
        ProcessedByUserId = processedBy;
        ProcessedAt = DateTime.UtcNow;
        AdminNotes = adminNotes;
        SetUpdatedAt();
    }

    public void Reject(Guid processedBy, string reason)
    {
        Status = "Rejected";
        ProcessedByUserId = processedBy;
        ProcessedAt = DateTime.UtcNow;
        AdminNotes = reason;
        SetUpdatedAt();
    }
}