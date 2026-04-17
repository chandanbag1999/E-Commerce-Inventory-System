namespace EcommerceInventory.Application.Features.SalesOrders.DTOs;

public class SalesOrderDto
{
    public Guid    Id             { get; set; }
    public string  SoNumber       { get; set; } = string.Empty;
    public string  Status         { get; set; } = string.Empty;
    public string  CustomerName   { get; set; } = string.Empty;
    public string? CustomerEmail  { get; set; }
    public string? CustomerPhone  { get; set; }
    public decimal Subtotal       { get; set; }
    public decimal TotalAmount    { get; set; }
    public string? Notes          { get; set; }
    public Guid    WarehouseId    { get; set; }
    public string  WarehouseName  { get; set; } = string.Empty;
    public Guid    CreatedBy      { get; set; }
    public string? CreatedByName  { get; set; }
    public Guid?   ApprovedBy     { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt   { get; set; }
    public DateTime? ShippedAt    { get; set; }
    public DateTime? DeliveredAt  { get; set; }
    public ShippingAddressDto? ShippingAddress { get; set; }
    public List<SalesOrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt     { get; set; }
    public DateTime UpdatedAt     { get; set; }
}

public class SalesOrderItemDto
{
    public Guid    Id          { get; set; }
    public Guid    ProductId   { get; set; }
    public string  ProductName { get; set; } = string.Empty;
    public string  ProductSku  { get; set; } = string.Empty;
    public int     Quantity    { get; set; }
    public decimal UnitPrice   { get; set; }
    public decimal Discount    { get; set; }
    public decimal LineTotal   { get; set; }
}

public class SalesOrderListDto
{
    public Guid    Id            { get; set; }
    public string  SoNumber      { get; set; } = string.Empty;
    public string  Status        { get; set; } = string.Empty;
    public string  CustomerName  { get; set; } = string.Empty;
    public decimal TotalAmount   { get; set; }
    public string  WarehouseName { get; set; } = string.Empty;
    public int     ItemCount     { get; set; }
    public DateTime CreatedAt    { get; set; }
}

public class ShippingAddressDto
{
    public string? Street  { get; set; }
    public string? City    { get; set; }
    public string? State   { get; set; }
    public string? Pincode { get; set; }
    public string? Country { get; set; }
}

public class AddSalesOrderItemRequest
{
    public Guid    ProductId { get; set; }
    public int     Quantity  { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount  { get; set; } = 0;
}
