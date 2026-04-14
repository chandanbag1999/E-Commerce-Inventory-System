namespace EcommerceInventory.Application.Features.Products.DTOs;

public record ProductDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string Slug,
    string Sku,
    string? Description,
    string? Barcode,
    decimal UnitPrice,
    decimal CostPrice,
    int ReorderLevel,
    int ReorderQty,
    string Status,
    decimal? WeightKg,
    DateTime CreatedAt,
    IReadOnlyList<ProductImageDto> Images
);

public record ProductListDto(
    Guid Id,
    string Name,
    string Sku,
    string CategoryName,
    decimal UnitPrice,
    string Status,
    string? PrimaryImageUrl,
    DateTime CreatedAt
);

public record ProductImageDto(
    Guid Id,
    string ImageUrl,
    bool IsPrimary,
    int DisplayOrder
);

public record CreateProductDto(
    Guid CategoryId,
    string Name,
    string Sku,
    string? Description = null,
    decimal UnitPrice = 0,
    decimal CostPrice = 0,
    int ReorderLevel = 0,
    int ReorderQty = 0,
    string? Barcode = null,
    decimal? WeightKg = null
);

public record UpdateProductDto(
    string Name,
    string? Description = null,
    decimal UnitPrice = 0,
    decimal CostPrice = 0,
    int ReorderLevel = 0,
    int ReorderQty = 0,
    decimal? WeightKg = null
);