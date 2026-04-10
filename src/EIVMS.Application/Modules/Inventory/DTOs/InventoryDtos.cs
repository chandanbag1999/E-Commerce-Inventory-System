using EIVMS.Domain.Enums.Inventory;

namespace EIVMS.Application.Modules.Inventory.DTOs;

public class WarehouseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public int? MaxCapacity { get; set; }
    public WarehouseStatus Status { get; set; }
    public bool IsDefault { get; set; }
    public int Priority { get; set; }
    public int TotalItems { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateWarehouseDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public int? MaxCapacity { get; set; }
    public bool IsDefault { get; set; }
    public int Priority { get; set; } = 1;
}

public class InventoryItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int DamagedQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public decimal? AverageCost { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class StockMovementDto
{
    public Guid Id { get; set; }
    public Guid InventoryItemId { get; set; }
    public Guid? WarehouseId { get; set; }
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StockReservationDto
{
    public Guid Id { get; set; }
    public Guid InventoryItemId { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public StockReservationStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StockTransferDto
{
    public Guid Id { get; set; }
    public Guid FromWarehouseId { get; set; }
    public string FromWarehouseName { get; set; } = string.Empty;
    public Guid ToWarehouseId { get; set; }
    public string ToWarehouseName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public StockTransferStatus Status { get; set; }
    public string? Reference { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStockTransferDto
{
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public class InventoryAlertDto
{
    public Guid Id { get; set; }
    public Guid? InventoryItemId { get; set; }
    public Guid? WarehouseId { get; set; }
    public InventoryAlertType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StockOperationDto
{
    public Guid InventoryItemId { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Reason { get; set; }
    public decimal? CostPrice { get; set; }
}

public class ReserveStockDto
{
    public Guid InventoryItemId { get; set; }
    public int Quantity { get; set; }
    public int ExpirationMinutes { get; set; } = 30;
}