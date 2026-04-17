using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;

namespace EcommerceInventory.Domain.Entities;

public class Product : BaseEntity, ISoftDelete
{
    public Guid          CategoryId    { get; private set; }
    public string        Name          { get; private set; } = string.Empty;
    public string        Slug          { get; private set; } = string.Empty;
    public string?       Description   { get; private set; }
    public string        Sku           { get; private set; } = string.Empty;
    public string?       Barcode       { get; private set; }
    public decimal       UnitPrice     { get; private set; }
    public decimal       CostPrice     { get; private set; }
    public int           ReorderLevel  { get; private set; } = 0;
    public int           ReorderQty    { get; private set; } = 0;
    public ProductStatus Status        { get; private set; } = ProductStatus.Active;
    public decimal?      WeightKg      { get; private set; }
    public Guid?         CreatedBy     { get; private set; }
    public DateTime?     DeletedAt     { get; set; }
    public bool          IsDeleted     => DeletedAt.HasValue;

    public Category                 Category { get; set; } = null!;
    public ICollection<ProductImage> Images  { get; set; } = new List<ProductImage>();
    public ICollection<Stock>        Stocks  { get; set; } = new List<Stock>();

    protected Product() { }

    public static Product Create(Guid categoryId, string name, string slug,
                                  string sku, decimal unitPrice, decimal costPrice,
                                  string? description = null, int reorderLevel = 0,
                                  int reorderQty = 0, string? barcode = null,
                                  decimal? weightKg = null, Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("SKU is required.");
        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative.");
        if (costPrice < 0)
            throw new DomainException("Cost price cannot be negative.");

        return new Product
        {
            CategoryId   = categoryId,
            Name         = name.Trim(),
            Slug         = slug.Trim().ToLower(),
            Sku          = sku.Trim().ToUpper(),
            Description  = description?.Trim(),
            UnitPrice    = unitPrice,
            CostPrice    = costPrice,
            ReorderLevel = reorderLevel,
            ReorderQty   = reorderQty,
            Barcode      = barcode?.Trim(),
            WeightKg     = weightKg,
            CreatedBy    = createdBy,
            Status       = ProductStatus.Active
        };
    }

    public void Update(Guid categoryId, string name, string? description,
                       decimal unitPrice, decimal costPrice, int reorderLevel,
                       int reorderQty, string? barcode, decimal? weightKg)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");
        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative.");
        if (costPrice < 0)
            throw new DomainException("Cost price cannot be negative.");

        CategoryId   = categoryId;
        Name         = name.Trim();
        Description  = description?.Trim();
        UnitPrice    = unitPrice;
        CostPrice    = costPrice;
        ReorderLevel = reorderLevel;
        ReorderQty   = reorderQty;
        Barcode      = barcode?.Trim();
        WeightKg     = weightKg;
        UpdatedAt    = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status    = ProductStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Discontinue()
    {
        Status    = ProductStatus.Discontinued;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}