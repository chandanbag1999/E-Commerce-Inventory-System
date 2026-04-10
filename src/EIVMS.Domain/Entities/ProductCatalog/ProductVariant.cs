using EIVMS.Domain.Common;

namespace EIVMS.Domain.Entities.ProductCatalog;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; private set; }

    public string SKU { get; private set; } = string.Empty;
    public string? Barcode { get; private set; }
    public string? Name { get; private set; }

    public decimal? Price { get; private set; }
    public decimal? CompareAtPrice { get; private set; }
    public decimal? CostPrice { get; private set; }

    public decimal? WeightKg { get; private set; }

    public int StockQuantity { get; private set; } = 0;
    public int ReservedQuantity { get; private set; } = 0;
    public int? LowStockThreshold { get; private set; }
    public bool TrackInventory { get; private set; } = true;
    public bool AllowBackorder { get; private set; } = false;

    public string? VariantAttributesJson { get; private set; }

    public string? ImageUrl { get; private set; }

    public bool IsActive { get; private set; } = true;
    public bool IsDefault { get; private set; } = false;

    public int DisplayOrder { get; private set; } = 0;

    public Product Product { get; private set; } = null!;

    private ProductVariant() { }

    public static ProductVariant Create(
        Guid productId, string sku, decimal? price = null,
        string? name = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required for variant");

        return new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            SKU = sku.ToUpperInvariant().Trim(),
            Name = name?.Trim(),
            Price = price,
            StockQuantity = 0,
            ReservedQuantity = 0,
            TrackInventory = true,
            IsActive = true,
            IsDefault = false,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string sku, string? name, string? barcode, decimal? price,
        decimal? compareAtPrice, decimal? costPrice, decimal? weightKg)
    {
        SKU = sku.ToUpperInvariant().Trim();
        Name = name?.Trim();
        Barcode = barcode?.Trim();
        Price = price;
        CompareAtPrice = compareAtPrice;
        CostPrice = costPrice;
        WeightKg = weightKg;
        SetUpdatedAt();
    }

    public void SetVariantAttributes(string? attributesJson)
    {
        VariantAttributesJson = attributesJson;
        SetUpdatedAt();
    }

    public void SetImage(string? imageUrl)
    {
        ImageUrl = imageUrl;
        SetUpdatedAt();
    }

    public void SetAsDefault() { IsDefault = true; SetUpdatedAt(); }
    public void UnsetDefault() { IsDefault = false; SetUpdatedAt(); }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Stock quantity cannot be negative");
        StockQuantity = quantity;
        SetUpdatedAt();
    }

    public void ReserveStock(int quantity)
    {
        if (quantity > AvailableStock)
            throw new InvalidOperationException($"Cannot reserve {quantity} units. Available: {AvailableStock}");
        ReservedQuantity += quantity;
        SetUpdatedAt();
    }

    public void ReleaseReservation(int quantity)
    {
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
        SetUpdatedAt();
    }

    public void SetInventorySettings(int? lowStockThreshold, bool trackInventory, bool allowBackorder)
    {
        LowStockThreshold = lowStockThreshold;
        TrackInventory = trackInventory;
        AllowBackorder = allowBackorder;
        SetUpdatedAt();
    }

    public void Activate() { IsActive = true; SetUpdatedAt(); }
    public void Deactivate() { IsActive = false; IsDefault = false; SetUpdatedAt(); }

    public int AvailableStock => StockQuantity - ReservedQuantity;
    public bool IsInStock => AllowBackorder || AvailableStock > 0;
    public bool IsLowStock => LowStockThreshold.HasValue && AvailableStock <= LowStockThreshold.Value && AvailableStock > 0;

    public decimal GetEffectivePrice(decimal productBasePrice) => Price ?? productBasePrice;
}
