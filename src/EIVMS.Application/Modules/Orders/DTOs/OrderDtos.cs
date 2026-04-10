using EIVMS.Domain.Enums.Orders;

namespace EIVMS.Application.Modules.Orders.DTOs;

public class CreateOrderDto
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.UPI;
    public OrderType OrderType { get; set; } = OrderType.Regular;
    public Guid? AddressId { get; set; }
    public OrderAddressDto? NewAddress { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public string? CouponCode { get; set; }
    public string? CustomerNotes { get; set; }
    public bool IsGift { get; set; } = false;
    public string? GiftMessage { get; set; }
    public DateTime? ScheduledDeliveryDate { get; set; }
    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }
}

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class OrderAddressDto
{
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
}

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderType OrderType { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCharges { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingContactName { get; set; } = string.Empty;
    public string ShippingContactPhone { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? CourierName { get; set; }
    public string? TrackingUrl { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? InvoiceUrl { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
    public bool CanBeCancelled { get; set; }
    public bool IsGift { get; set; }
    public string? GiftMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CustomerNotes { get; set; }
}

public class OrderItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string? ProductImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TotalPrice { get; set; }
    public string? WarehouseName { get; set; }
    public int ReturnedQuantity { get; set; }
    public bool IsFullyReturned { get; set; }
}

public class OrderStatusHistoryDto
{
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsCustomerVisible { get; set; }
}

public class OrderListResponseDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public string? PrimaryProductImage { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}

public class UpdateOrderStatusDto
{
    public OrderStatus NewStatus { get; set; }
    public string? Notes { get; set; }
}

public class ConfirmPaymentDto
{
    public string PaymentTransactionId { get; set; } = string.Empty;
    public string? GatewayResponse { get; set; }
}

public class ShipOrderDto
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string CourierName { get; set; } = string.Empty;
    public string? TrackingUrl { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}

public class CancelOrderDto
{
    public CancellationReason Reason { get; set; }
    public string? Notes { get; set; }
}

public class ReturnOrderDto
{
    public ReturnReason Reason { get; set; }
    public string? Notes { get; set; }
    public List<ReturnItemDto> Items { get; set; } = new();
}

public class ReturnItemDto
{
    public Guid OrderItemId { get; set; }
    public int Quantity { get; set; }
    public string? ProofImageUrl { get; set; }
}

public class OrderFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? UserId { get; set; }
    public OrderStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? OrderNumber { get; set; }
    public string? SearchQuery { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public Guid? VendorId { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
}

public class OrderSummaryDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class OrderPricingDto
{
    public decimal SubTotal { get; set; }
    public decimal ShippingCharges { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CouponCode { get; set; }
    public decimal? CouponDiscount { get; set; }
    public bool IsFreeShipping { get; set; }
}