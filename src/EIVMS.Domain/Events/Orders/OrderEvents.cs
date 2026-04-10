using MediatR;

namespace EIVMS.Domain.Events.Orders;

public record OrderCreatedEvent(Guid OrderId, string OrderNumber, Guid UserId, decimal TotalAmount, DateTime CreatedAt) : INotification;
public record OrderConfirmedEvent(Guid OrderId, string OrderNumber, Guid UserId, string? PaymentTransactionId) : INotification;
public record OrderShippedEvent(Guid OrderId, string OrderNumber, Guid UserId, string TrackingNumber, string CourierName, string? TrackingUrl, DateTime? EstimatedDelivery) : INotification;
public record OrderDeliveredEvent(Guid OrderId, string OrderNumber, Guid UserId, DateTime DeliveredAt) : INotification;
public record OrderCancelledEvent(Guid OrderId, string OrderNumber, Guid UserId, string CancellationReason, bool ShouldRefund) : INotification;
public record OrderReturnRequestedEvent(Guid OrderId, string OrderNumber, Guid UserId, string ReturnReason) : INotification;
public record PaymentFailedEvent(Guid OrderId, string OrderNumber, Guid UserId, string FailureReason) : INotification;