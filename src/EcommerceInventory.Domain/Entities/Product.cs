using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;


// Product entity representing an item in the inventory
public class Product : AuditableEntity, ISoftDelete
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal UnitPrice { get; set; } = 0;
    public decimal CostPrice { get; set; } = 0;
    public int ReorderLevel { get; set; } = 0;
    public int ReorderQty { get; set; } = 0;
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public decimal? WeightKg { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
    public ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();


    // Factory method to create a new product with validation
    public static Product Create(
        Guid categoryId,
        string name,
        string sku,
        decimal unitPrice,
        decimal costPrice,
        string? description = null,
        int reorderLevel = 0,
        int reorderQty = 0,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("Product SKU cannot be empty");
        
        if (unitPrice < 0)
            throw new DomainException("Unit price must be >= 0");
        
        if (costPrice < 0)
            throw new DomainException("Cost price must be >= 0");

        return new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Name = name.Trim(),
            Sku = sku.Trim().ToUpper(),
            Slug = GenerateSlug(name, sku),
            Description = description?.Trim(),
            UnitPrice = unitPrice,
            CostPrice = costPrice,
            ReorderLevel = reorderLevel,
            ReorderQty = reorderQty,
            Status = ProductStatus.Active,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string GenerateSlug(string name, string sku)
    {
        var nameSlug = name.Trim().ToLower().Replace(" ", "-");
        var skuSuffix = sku.Substring(Math.Min(sku.Length, 4)).ToLower();
        return $"{nameSlug}-{skuSuffix}";
    }

    // Method to update product details with validation
    public void Update(
        string name,
        decimal unitPrice,
        decimal costPrice,
        string? description = null,
        int reorderLevel = 0,
        int reorderQty = 0,
        decimal? weightKg = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty");

        Name = name.Trim();
        UnitPrice = unitPrice;
        CostPrice = costPrice;
        Description = description?.Trim();
        ReorderLevel = reorderLevel;
        ReorderQty = reorderQty;
        WeightKg = weightKg;
        UpdatedAt = DateTime.UtcNow;
    }

    // Method to soft delete the product
    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Method to activate the product
    public void Activate()
    {
        Status = ProductStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    // Method to deactivate the product
    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }
}
