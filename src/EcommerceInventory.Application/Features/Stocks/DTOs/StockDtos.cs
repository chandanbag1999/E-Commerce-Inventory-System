namespace EcommerceInventory.Application.Features.Stocks.DTOs;

public record StockDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    Guid WarehouseId,
    string WarehouseName,
    string WarehouseCode,
    int Quantity,
    int ReservedQty,
    int AvailableQty,
    DateTime? LastCountedAt,
    DateTime UpdatedAt);

public record LowStockAlertDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    Guid WarehouseId,
    string WarehouseName,
    int CurrentQty,
    int AvailableQty,
    int ReorderLevel,
    int ReorderQty,
    int Deficit);

public record AdjustStockDto(
    Guid ProductId,
    Guid WarehouseId,
    string AdjustmentType,
    int Quantity,
    string Reason);

public record AdjustStockResponseDto(
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    int AvailableQty,
    int ReservedQty,
    string? LastMovementType,
    DateTime UpdatedAt);
