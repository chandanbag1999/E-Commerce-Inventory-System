using EIVMS.Domain.Enums.ProductCatalog;

namespace EIVMS.Application.Modules.ProductCatalog.DTOs.Product;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public Guid CategoryId { get; set; }
    public ProductType Type { get; set; } = ProductType.Physical;
    public decimal BasePrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string Currency { get; set; } = "INR";
    public PricingType PricingType { get; set; } = PricingType.Fixed;
    public decimal TaxRate { get; set; } = 18;
    public string? HsnCode { get; set; }
    public bool IsTaxInclusive { get; set; } = false;
    public decimal? WeightKg { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, string> Attributes { get; set; } = new();
    public CreateVariantDto? DefaultVariant { get; set; }
}

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public Guid CategoryId { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public PricingType PricingType { get; set; } = PricingType.Fixed;
    public decimal TaxRate { get; set; }
    public string? HsnCode { get; set; }
    public bool IsTaxInclusive { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, string> Attributes { get; set; } = new();
}
