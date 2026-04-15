namespace EcommerceInventory.Application.Features.SalesOrders.DTOs;

public record SalesOrderDto(
    Guid Id,
    string SoNumber,
    string CustomerName,
    string? CustomerEmail,
    string? CustomerPhone,
    Guid WarehouseId,
    string WarehouseName,
    string Status,
    decimal Subtotal,
    decimal TotalAmount,
    string? Notes,
    string? ShippingAddressJson,
    Guid? ApprovedBy,
    DateTime? ApprovedAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    List<SalesOrderItemDto> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record SalesOrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal Total);

public record CreateSalesOrderDto(
    Guid WarehouseId,
    string? CustomerName,
    string? CustomerEmail,
    string? CustomerPhone,
    string? Notes,
    string? ShippingAddressJson,
    List<CreateSalesOrderItemDto> Items);

public record CreateSalesOrderItemDto(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal Discount = 0);

public record AddSalesOrderItemDto(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal Discount = 0);

public record SalesOrderListDto(
    Guid Id,
    string SoNumber,
    string CustomerName,
    string WarehouseName,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt);
