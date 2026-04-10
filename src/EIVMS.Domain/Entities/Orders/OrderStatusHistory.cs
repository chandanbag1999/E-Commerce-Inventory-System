using EIVMS.Domain.Common;
using EIVMS.Domain.Enums.Orders;

namespace EIVMS.Domain.Entities.Orders;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; private set; }
    public OrderStatus FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public string? Notes { get; private set; }
    public Guid? PerformedByUserId { get; private set; }
    public string? PerformedByName { get; private set; }
    public bool IsCustomerVisible { get; private set; } = true;
    public Order Order { get; private set; } = null!;

    private OrderStatusHistory() { }

    public static OrderStatusHistory Create(Guid orderId, OrderStatus fromStatus, OrderStatus toStatus,
        string? notes = null, Guid? performedByUserId = null, bool isCustomerVisible = true)
    {
        return new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Notes = notes,
            PerformedByUserId = performedByUserId,
            IsCustomerVisible = isCustomerVisible,
            CreatedAt = DateTime.UtcNow
        };
    }
}