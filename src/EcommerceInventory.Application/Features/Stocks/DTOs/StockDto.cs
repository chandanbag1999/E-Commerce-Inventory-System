namespace EcommerceInventory.Application.Features.Stocks.DTOs;

public class StockDto
{
    public Guid    Id           { get; set; }
    public Guid    ProductId    { get; set; }
    public string  ProductName  { get; set; } = string.Empty;
    public string  ProductSku   { get; set; } = string.Empty;
    public string? PrimaryImage { get; set; }
    public Guid    WarehouseId  { get; set; }
    public string  WarehouseName { get; set; } = string.Empty;
    public string  WarehouseCode { get; set; } = string.Empty;
    public int     Quantity     { get; set; }
    public int     ReservedQty  { get; set; }
    public int     AvailableQty { get; set; }
    public int     ReorderLevel { get; set; }
    public bool    IsLowStock   { get; set; }
    public DateTime? LastCountedAt { get; set; }
    public DateTime UpdatedAt   { get; set; }
}

public class LowStockAlertDto
{
    public Guid   ProductId    { get; set; }
    public string ProductName  { get; set; } = string.Empty;
    public string ProductSku   { get; set; } = string.Empty;
    public Guid   WarehouseId  { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int    CurrentQty   { get; set; }
    public int    AvailableQty { get; set; }
    public int    ReservedQty  { get; set; }
    public int    ReorderLevel { get; set; }
    public int    ReorderQty   { get; set; }
    public int    Deficit      { get; set; }
}

public class StockAdjustmentResultDto
{
    public Guid   StockId      { get; set; }
    public Guid   ProductId    { get; set; }
    public string ProductName  { get; set; } = string.Empty;
    public Guid   WarehouseId  { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int    QuantityBefore { get; set; }
    public int    QuantityAfter  { get; set; }
    public int    AdjustedBy     { get; set; }
    public string AdjustmentType { get; set; } = string.Empty;
    public string Reason         { get; set; } = string.Empty;
    public int    AvailableQty   { get; set; }
    public int    ReservedQty    { get; set; }
}
