namespace EcommerceInventory.Application.Features.PurchaseOrders.DTOs;

public class PurchaseOrderDto
{
    public Guid     Id                 { get; set; }
    public string   PoNumber           { get; set; } = string.Empty;
    public string   Status             { get; set; } = string.Empty;
    public decimal  TotalAmount        { get; set; }
    public string?  Notes              { get; set; }
    public Guid     SupplierId         { get; set; }
    public string   SupplierName       { get; set; } = string.Empty;
    public Guid     WarehouseId        { get; set; }
    public string   WarehouseName      { get; set; } = string.Empty;
    public Guid     CreatedBy          { get; set; }
    public string?  CreatedByName      { get; set; }
    public Guid?    ApprovedBy         { get; set; }
    public string?  ApprovedByName     { get; set; }
    public DateTime? ApprovedAt        { get; set; }
    public DateTime? ExpectedDeliveryAt { get; set; }
    public DateTime? ReceivedAt        { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt          { get; set; }
    public DateTime UpdatedAt          { get; set; }
}

public class PurchaseOrderItemDto
{
    public Guid    Id               { get; set; }
    public Guid    ProductId        { get; set; }
    public string  ProductName      { get; set; } = string.Empty;
    public string  ProductSku       { get; set; } = string.Empty;
    public int     QuantityOrdered  { get; set; }
    public int     QuantityReceived { get; set; }
    public decimal UnitCost         { get; set; }
    public decimal TotalCost        { get; set; }
}

public class PurchaseOrderListDto
{
    public Guid    Id            { get; set; }
    public string  PoNumber      { get; set; } = string.Empty;
    public string  Status        { get; set; } = string.Empty;
    public decimal TotalAmount   { get; set; }
    public string  SupplierName  { get; set; } = string.Empty;
    public string  WarehouseName { get; set; } = string.Empty;
    public int     ItemCount     { get; set; }
    public DateTime? ExpectedDeliveryAt { get; set; }
    public DateTime CreatedAt    { get; set; }
}

public class AddPurchaseOrderItemRequest
{
    public Guid    ProductId       { get; set; }
    public int     QuantityOrdered { get; set; }
    public decimal UnitCost        { get; set; }
}

public class ReceivePurchaseOrderItemRequest
{
    public Guid ItemId           { get; set; }
    public int  QuantityReceived { get; set; }
}
