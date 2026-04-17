using EcommerceInventory.Application.Features.Products.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<ProductDto>
{
    public Guid    CategoryId   { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string  Sku          { get; set; } = string.Empty;
    public string? Description  { get; set; }
    public decimal UnitPrice    { get; set; }
    public decimal CostPrice    { get; set; }
    public int     ReorderLevel { get; set; } = 0;
    public int     ReorderQty   { get; set; } = 0;
    public string? Barcode      { get; set; }
    public decimal? WeightKg    { get; set; }
    public Guid?   CreatedBy    { get; set; }
    public List<ProductImageUpload> Images { get; set; } = new();
}

public class ProductImageUpload
{
    public Stream FileStream  { get; set; } = null!;
    public string FileName    { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
