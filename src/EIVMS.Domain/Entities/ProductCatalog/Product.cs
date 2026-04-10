using EIVMS.Domain.Common;
using EIVMS.Domain.Enums.ProductCatalog;

namespace EIVMS.Domain.Entities.ProductCatalog;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? ShortDescription { get; private set; }
    public string? FullDescription { get; private set; }
    public string? SKU { get; private set; }
    public string? Barcode { get; private set; }
    public string? Brand { get; private set; }
    public string? Model { get; private set; }

    public Guid CategoryId { get; private set; }
    public ProductType Type { get; private set; } = ProductType.Physical;
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;

    public decimal BasePrice { get; private set; }
    public decimal? CompareAtPrice { get; private set; }
    public string Currency { get; private set; } = "INR";
    public PricingType PricingType { get; private set; } = PricingType.Fixed;

    public decimal TaxRate { get; private set; } = 0;
    public string? HsnCode { get; private set; }
    public bool IsTaxInclusive { get; private set; } = false;

    public decimal? WeightKg { get; private set; }
    public decimal? LengthCm { get; private set; }
    public decimal? WidthCm { get; private set; }
    public decimal? HeightCm { get; private set; }

    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? MetaKeywords { get; private set; }

    public string? Tags { get; private set; }
    public string? AttributesJson { get; private set; }

    public Guid? VendorId { get; private set; }

    public bool IsFeatured { get; private set; } = false;
    public bool IsDigitalDownload { get; private set; } = false;

    public DateTime? PublishedAt { get; private set; }

    public bool IsDeleted { get; private set; } = false;
    public DateTime? DeletedAt { get; private set; }

    public Guid CreatedByUserId { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    public Category Category { get; private set; } = null!;
    public ICollection<ProductVariant> Variants { get; private set; } = new List<ProductVariant>();
    public ICollection<ProductMedia> Media { get; private set; } = new List<ProductMedia>();
    public ICollection<ProductAttributeValue> AttributeValues { get; private set; } = new List<ProductAttributeValue>();
    public ICollection<ProductTag> ProductTags { get; private set; } = new List<ProductTag>();

    private Product() { }

    public static Product Create(
        string name,
        string slug,
        Guid categoryId,
        decimal basePrice,
        Guid createdByUserId,
        ProductType type = ProductType.Physical,
        string? shortDescription = null,
        Guid? vendorId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required");

        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative");

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = GenerateSlug(slug),
            CategoryId = categoryId,
            BasePrice = basePrice,
            CreatedByUserId = createdByUserId,
            Type = type,
            ShortDescription = shortDescription,
            VendorId = vendorId,
            Status = ProductStatus.Draft,
            Currency = "INR",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateBasicInfo(
        string name, string slug, string? shortDescription,
        string? fullDescription, string? brand, string? model, Guid updatedByUserId)
    {
        Name = name.Trim();
        Slug = GenerateSlug(slug);
        ShortDescription = shortDescription;
        FullDescription = fullDescription;
        Brand = brand;
        Model = model;
        UpdatedByUserId = updatedByUserId;
        SetUpdatedAt();
    }

    public void UpdatePricing(
        decimal basePrice, decimal? compareAtPrice, decimal taxRate,
        bool isTaxInclusive, string? hsnCode, PricingType pricingType)
    {
        BasePrice = basePrice;
        CompareAtPrice = compareAtPrice;
        TaxRate = taxRate;
        IsTaxInclusive = isTaxInclusive;
        HsnCode = hsnCode;
        PricingType = pricingType;
        SetUpdatedAt();
    }

    public void UpdatePhysicalAttributes(decimal? weightKg, decimal? lengthCm, decimal? widthCm, decimal? heightCm)
    {
        WeightKg = weightKg;
        LengthCm = lengthCm;
        WidthCm = widthCm;
        HeightCm = heightCm;
        SetUpdatedAt();
    }

    public void UpdateSeo(string? metaTitle, string? metaDescription, string? metaKeywords)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
        SetUpdatedAt();
    }

    public void SetAttributesJson(string? json)
    {
        AttributesJson = json;
        SetUpdatedAt();
    }

    public void Publish()
    {
        if (Status == ProductStatus.Archived)
            throw new InvalidOperationException("Archived products cannot be published");

        if (!Variants.Any())
            throw new InvalidOperationException("Product must have at least one variant before publishing");

        Status = ProductStatus.Active;
        PublishedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Unpublish()
    {
        Status = ProductStatus.Inactive;
        SetUpdatedAt();
    }

    public void Archive()
    {
        Status = ProductStatus.Archived;
        SetUpdatedAt();
    }

    public void Activate()
    {
        if (Status == ProductStatus.Archived)
            throw new InvalidOperationException("Cannot activate archived product");

        Status = ProductStatus.Active;
        SetUpdatedAt();
    }

    public void SetFeatured(bool isFeatured)
    {
        IsFeatured = isFeatured;
        SetUpdatedAt();
    }

    public void SoftDelete(Guid deletedByUserId)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        Status = ProductStatus.Archived;
        UpdatedByUserId = deletedByUserId;
        SetUpdatedAt();
    }

    public decimal EffectivePrice => BasePrice;
    public decimal PriceWithTax => IsTaxInclusive ? BasePrice : BasePrice + (BasePrice * TaxRate / 100);
    public decimal? DiscountPercentage => CompareAtPrice.HasValue && CompareAtPrice.Value > 0
        ? Math.Round((CompareAtPrice.Value - BasePrice) / CompareAtPrice.Value * 100, 2) : null;
    public bool IsPublished => Status == ProductStatus.Active;

    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var slug = input.ToLowerInvariant().Trim();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }
}
