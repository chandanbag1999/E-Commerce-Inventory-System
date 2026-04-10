using EIVMS.Domain.Enums.ProductCatalog;

namespace EIVMS.Application.Modules.ProductCatalog.DTOs.Product;

public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? SKU { get; set; }
    public string? Brand { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public ProductType Type { get; set; }
    public ProductStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public decimal BasePrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal PriceWithTax { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public bool IsTaxInclusive { get; set; }
    public decimal? WeightKg { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<ProductMediaDto> Media { get; set; } = new();
    public List<ProductVariantResponseDto> Variants { get; set; } = new();
    public int TotalVariants { get; set; }
    public int TotalStock { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class ProductListResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public ProductStatus Status { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalStock { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductMediaDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public MediaType Type { get; set; }
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public string? MimeType { get; set; }
}

public class ProductVariantResponseDto
{
    public Guid Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Barcode { get; set; }
    public decimal? Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int AvailableStock { get; set; }
    public bool IsInStock { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, string> VariantAttributes { get; set; } = new();
    public int DisplayOrder { get; set; }
}

public class ProductFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchQuery { get; set; }
    public Guid? CategoryId { get; set; }
    public bool IncludeSubCategories { get; set; } = true;
    public string? Brand { get; set; }
    public ProductStatus? Status { get; set; }
    public ProductType? Type { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStockOnly { get; set; }
    public bool? FeaturedOnly { get; set; }
    public Guid? VendorId { get; set; }
    public List<string> Tags { get; set; } = new();
    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
}
