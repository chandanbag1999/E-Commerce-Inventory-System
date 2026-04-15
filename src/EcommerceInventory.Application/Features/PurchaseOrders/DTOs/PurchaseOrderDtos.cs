namespace EcommerceInventory.Application.Features.PurchaseOrders.DTOs;

public record PurchaseOrderDto(
    Guid Id,
    string PoNumber,
    Guid SupplierId,
    string SupplierName,
    Guid WarehouseId,
    string WarehouseName,
    string Status,
    decimal TotalAmount,
    string? Notes,
    DateTime? ExpectedDeliveryAt,
    Guid? ApprovedBy,
    DateTime? ApprovedAt,
    DateTime? ReceivedAt,
    List<PurchaseOrderItemDto> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record PurchaseOrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int QuantityOrdered,
    int QuantityReceived,
    decimal UnitCost,
    decimal TotalCost);

public record CreatePurchaseOrderDto(
    Guid SupplierId,
    Guid WarehouseId,
    DateTime? ExpectedDeliveryAt,
    string? Notes,
    List<CreatePurchaseOrderItemDto> Items);

public record CreatePurchaseOrderItemDto(
    Guid ProductId,
    int QuantityOrdered,
    decimal UnitCost);

public record AddPurchaseOrderItemDto(
    Guid ProductId,
    int QuantityOrdered,
    decimal UnitCost);

public record ReceivePurchaseOrderDto(
    List<ReceivePurchaseOrderItemDto> Items);

public record ReceivePurchaseOrderItemDto(
    Guid ItemId,
    int QuantityReceived);

public record PurchaseOrderListDto(
    Guid Id,
    string PoNumber,
    string SupplierName,
    string WarehouseName,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt);
