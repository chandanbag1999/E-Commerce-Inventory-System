using EIVMS.Domain.Common;
using EIVMS.Domain.Enums.Orders;

namespace EIVMS.Domain.Entities.Orders;

public class Order : BaseEntity
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public OrderType Type { get; private set; } = OrderType.Regular;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;
    public PaymentMethod PaymentMethod { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingCharges { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = "INR";
    public string? CouponCode { get; private set; }
    public decimal? CouponDiscountAmount { get; private set; }
    public string ShippingAddressLine1 { get; private set; } = string.Empty;
    public string ShippingAddressLine2 { get; private set; } = string.Empty;
    public string ShippingCity { get; private set; } = string.Empty;
    public string ShippingState { get; private set; } = string.Empty;
    public string ShippingCountry { get; private set; } = string.Empty;
    public string ShippingPinCode { get; private set; } = string.Empty;
    public string ShippingContactName { get; private set; } = string.Empty;
    public string ShippingContactPhone { get; private set; } = string.Empty;
    public string? PaymentTransactionId { get; private set; }
    public DateTime? PaymentCompletedAt { get; private set; }
    public string? PaymentGatewayResponse { get; private set; }
    public string? TrackingNumber { get; private set; }
    public string? CourierName { get; private set; }
    public string? TrackingUrl { get; private set; }
    public DateTime? EstimatedDeliveryDate { get; private set; }
    public DateTime? ActualDeliveryDate { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public CancellationReason? CancellationReason { get; private set; }
    public string? CancellationNotes { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public ReturnReason? ReturnReason { get; private set; }
    public string? ReturnNotes { get; private set; }
    public DateTime? ReturnRequestedAt { get; private set; }
    public string? CustomerNotes { get; private set; }
    public string? InternalNotes { get; private set; }
    public DateTime? ScheduledDeliveryDate { get; private set; }
    public bool IsGift { get; private set; } = false;
    public string? GiftMessage { get; private set; }
    public Guid? VendorId { get; private set; }
    public string? InvoiceNumber { get; private set; }
    public string? InvoiceUrl { get; private set; }
    public DateTime? InvoiceGeneratedAt { get; private set; }
    public bool IsDeleted { get; private set; } = false;

    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; private set; } = new List<OrderStatusHistory>();
    public ICollection<OrderReturnItem> ReturnItems { get; private set; } = new List<OrderReturnItem>();

    private Order() { }

    public static Order Create(Guid userId, string idempotencyKey, PaymentMethod paymentMethod,
        OrderType orderType = OrderType.Regular, string currency = "INR", string? customerNotes = null,
        bool isGift = false, string? giftMessage = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID is required");
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency key is required");

        return new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            IdempotencyKey = idempotencyKey,
            Type = orderType,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            PaymentMethod = paymentMethod,
            Currency = currency,
            CustomerNotes = customerNotes,
            IsGift = isGift,
            GiftMessage = giftMessage,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetShippingAddress(string addressLine1, string addressLine2, string city, string state,
        string country, string pinCode, string contactName, string contactPhone)
    {
        ShippingAddressLine1 = addressLine1;
        ShippingAddressLine2 = addressLine2;
        ShippingCity = city;
        ShippingState = state;
        ShippingCountry = country;
        ShippingPinCode = pinCode;
        ShippingContactName = contactName;
        ShippingContactPhone = contactPhone;
        SetUpdatedAt();
    }

    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to a non-pending order");
        Items.Add(item);
        RecalculateTotals();
    }

    public void SetPricing(decimal subtotal, decimal shippingCharges, decimal taxAmount, decimal discountAmount = 0,
        string? couponCode = null, decimal? couponDiscount = null)
    {
        SubTotal = subtotal;
        ShippingCharges = shippingCharges;
        TaxAmount = taxAmount;
        DiscountAmount = discountAmount;
        CouponCode = couponCode;
        CouponDiscountAmount = couponDiscount;
        TotalAmount = subtotal + shippingCharges + taxAmount - discountAmount;
        SetUpdatedAt();
    }

    private static readonly Dictionary<OrderStatus, List<OrderStatus>> ValidTransitions = new()
    {
        [OrderStatus.Pending] = new() { OrderStatus.Confirmed, OrderStatus.Cancelled, OrderStatus.Failed },
        [OrderStatus.Confirmed] = new() { OrderStatus.Processing, OrderStatus.Cancelled },
        [OrderStatus.Processing] = new() { OrderStatus.Packed, OrderStatus.Cancelled },
        [OrderStatus.Packed] = new() { OrderStatus.Shipped, OrderStatus.Cancelled },
        [OrderStatus.Shipped] = new() { OrderStatus.OutForDelivery, OrderStatus.Cancelled },
        [OrderStatus.OutForDelivery] = new() { OrderStatus.Delivered, OrderStatus.Cancelled },
        [OrderStatus.Delivered] = new() { OrderStatus.ReturnRequested },
        [OrderStatus.ReturnRequested] = new() { OrderStatus.Returned, OrderStatus.Delivered },
        [OrderStatus.Returned] = new() { OrderStatus.Refunded },
        [OrderStatus.Cancelled] = new(),
        [OrderStatus.Refunded] = new(),
        [OrderStatus.Failed] = new() { OrderStatus.Pending }
    };

    private void TransitionTo(OrderStatus newStatus, string? notes = null, Guid? performedBy = null)
    {
        if (!ValidTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidOperationException($"Invalid transition: {Status} → {newStatus}");

        var previousStatus = Status;
        Status = newStatus;
        StatusHistory.Add(OrderStatusHistory.Create(Id, previousStatus, newStatus, notes, performedBy));
        SetUpdatedAt();
    }

    public void Confirm(string paymentTransactionId, string? gatewayResponse = null, Guid? performedBy = null)
    {
        if (PaymentMethod != PaymentMethod.CashOnDelivery && string.IsNullOrWhiteSpace(paymentTransactionId))
            throw new ArgumentException("Payment transaction ID required");

        PaymentTransactionId = paymentTransactionId;
        PaymentStatus = PaymentStatus.Paid;
        PaymentCompletedAt = DateTime.UtcNow;
        PaymentGatewayResponse = gatewayResponse;
        if (PaymentMethod == PaymentMethod.CashOnDelivery) PaymentStatus = PaymentStatus.Pending;

        TransitionTo(OrderStatus.Confirmed, "Payment received, inventory reserved", performedBy);
    }

    public void StartProcessing(Guid? performedBy = null) => TransitionTo(OrderStatus.Processing, "Order picked up for packing", performedBy);
    public void MarkAsPacked(Guid? performedBy = null) => TransitionTo(OrderStatus.Packed, "Order packed, ready for courier pickup", performedBy);

    public void MarkAsShipped(string trackingNumber, string courierName, string? trackingUrl = null,
        DateTime? estimatedDelivery = null, Guid? performedBy = null)
    {
        TrackingNumber = trackingNumber;
        CourierName = courierName;
        TrackingUrl = trackingUrl;
        EstimatedDeliveryDate = estimatedDelivery;
        ShippedAt = DateTime.UtcNow;
        TransitionTo(OrderStatus.Shipped, $"Shipped via {courierName}. Tracking: {trackingNumber}", performedBy);
    }

    public void MarkOutForDelivery(Guid? performedBy = null) => TransitionTo(OrderStatus.OutForDelivery, "Order is out for delivery", performedBy);
    public void MarkAsDelivered(Guid? performedBy = null)
    {
        ActualDeliveryDate = DateTime.UtcNow;
        if (PaymentMethod == PaymentMethod.CashOnDelivery) PaymentStatus = PaymentStatus.Paid;
        TransitionTo(OrderStatus.Delivered, "Order delivered successfully", performedBy);
    }

    public void Cancel(CancellationReason reason, string? notes = null, Guid? cancelledByUserId = null)
    {
        CancellationReason = reason;
        CancellationNotes = notes;
        CancelledAt = DateTime.UtcNow;
        CancelledByUserId = cancelledByUserId;
        TransitionTo(OrderStatus.Cancelled, $"Cancelled: {reason}. {notes}", cancelledByUserId);
    }

    public void MarkAsFailed(string? reason = null)
    {
        PaymentStatus = PaymentStatus.Failed;
        TransitionTo(OrderStatus.Failed, $"Payment failed: {reason}");
    }

    public void RequestReturn(ReturnReason reason, string? notes = null, Guid? performedBy = null)
    {
        ReturnReason = reason;
        ReturnNotes = notes;
        ReturnRequestedAt = DateTime.UtcNow;
        TransitionTo(OrderStatus.ReturnRequested, $"Return requested: {reason}", performedBy);
    }

    public void CompleteReturn(Guid? performedBy = null) => TransitionTo(OrderStatus.Returned, "Return received at warehouse", performedBy);
    public void ProcessRefund(Guid? performedBy = null)
    {
        PaymentStatus = PaymentStatus.Refunded;
        TransitionTo(OrderStatus.Refunded, "Refund processed", performedBy);
    }

    public void SetInvoice(string invoiceNumber, string invoiceUrl)
    {
        InvoiceNumber = invoiceNumber;
        InvoiceUrl = invoiceUrl;
        InvoiceGeneratedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void UpdateTracking(string? trackingNumber, string? trackingUrl, DateTime? estimatedDelivery)
    {
        if (trackingNumber != null) TrackingNumber = trackingNumber;
        if (trackingUrl != null) TrackingUrl = trackingUrl;
        if (estimatedDelivery.HasValue) EstimatedDeliveryDate = estimatedDelivery;
        SetUpdatedAt();
    }

    public void UpdateInternalNotes(string notes) { InternalNotes = notes; SetUpdatedAt(); }

    public bool CanBeCancelled => Status is OrderStatus.Pending or OrderStatus.Confirmed or OrderStatus.Processing;
    public bool IsTerminal => Status is OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Refunded or OrderStatus.Failed;

    private static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(100000, 999999);
        return $"ORD-{timestamp}-{random}";
    }

    private void RecalculateTotals()
    {
        SubTotal = Items.Sum(i => i.TotalPrice);
        TotalAmount = SubTotal + ShippingCharges + TaxAmount - DiscountAmount;
    }
}