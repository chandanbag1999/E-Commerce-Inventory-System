namespace EcommerceInventory.Application.Features.Products.DTOs;

public class ProductDto
{
    public Guid    Id            { get; set; }
    public Guid    CategoryId    { get; set; }
    public string  CategoryName  { get; set; } = string.Empty;
    public string  Name          { get; set; } = string.Empty;
    public string  Slug          { get; set; } = string.Empty;
    public string? Description   { get; set; }
    public string  Sku           { get; set; } = string.Empty;
    public string? Barcode       { get; set; }
    public decimal UnitPrice     { get; set; }
    public decimal CostPrice     { get; set; }
    public int     ReorderLevel  { get; set; }
    public int     ReorderQty    { get; set; }
    public string  Status        { get; set; } = string.Empty;
    public decimal? WeightKg     { get; set; }
    public string? PrimaryImage  { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
    public int     TotalStock    { get; set; }
    public DateTime CreatedAt    { get; set; }
    public DateTime UpdatedAt    { get; set; }
}

public class ProductImageDto
{
    public Guid   Id           { get; set; }
    public string ImageUrl     { get; set; } = string.Empty;
    public bool   IsPrimary    { get; set; }
    public int    DisplayOrder { get; set; }
}

public class ProductListDto
{
    public Guid    Id           { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string  Sku          { get; set; } = string.Empty;
    public string  CategoryName { get; set; } = string.Empty;
    public decimal UnitPrice    { get; set; }
    public decimal CostPrice    { get; set; }
    public string  Status       { get; set; } = string.Empty;
    public string? PrimaryImage { get; set; }
    public int     TotalStock   { get; set; }
    public DateTime CreatedAt   { get; set; }
}
