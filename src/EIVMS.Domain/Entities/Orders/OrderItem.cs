using EIVMS.Domain.Common;

namespace EIVMS.Domain.Entities.Orders;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public string SKU { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string? VariantName { get; private set; }
    public string? ProductImageUrl { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountedPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalPrice => DiscountedPrice * Quantity;
    public decimal TotalTax => TaxAmount * Quantity;
    public Guid? WarehouseId { get; private set; }
    public string? WarehouseName { get; private set; }
    public Guid? VendorId { get; private set; }
    public int ReturnedQuantity { get; private set; } = 0;
    public bool IsFullyReturned => ReturnedQuantity >= Quantity;
    public Order Order { get; private set; } = null!;

    private OrderItem() { }

    public static OrderItem Create(Guid orderId, Guid productId, string sku, string productName,
        decimal unitPrice, decimal discountedPrice, int quantity, decimal taxRate,
        Guid? variantId = null, string? variantName = null, string? productImageUrl = null,
        Guid? warehouseId = null, Guid? vendorId = null)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
        if (unitPrice < 0) throw new ArgumentException("Unit price cannot be negative");

        var taxAmount = discountedPrice * (taxRate / 100);
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            VariantId = variantId,
            SKU = sku.ToUpperInvariant(),
            ProductName = productName.Trim(),
            VariantName = variantName,
            ProductImageUrl = productImageUrl,
            UnitPrice = unitPrice,
            DiscountedPrice = discountedPrice,
            Quantity = quantity,
            TaxRate = taxRate,
            TaxAmount = taxAmount,
            WarehouseId = warehouseId,
            VendorId = vendorId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddReturnedQuantity(int quantity)
    {
        if (ReturnedQuantity + quantity > Quantity)
            throw new InvalidOperationException($"Cannot return more than ordered quantity ({Quantity})");
        ReturnedQuantity += quantity;
        SetUpdatedAt();
    }

    public void AssignWarehouse(Guid warehouseId, string warehouseName)
    {
        WarehouseId = warehouseId;
        WarehouseName = warehouseName;
        SetUpdatedAt();
    }
}