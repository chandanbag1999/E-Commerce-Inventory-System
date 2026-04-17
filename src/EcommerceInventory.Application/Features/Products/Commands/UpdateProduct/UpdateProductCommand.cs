using EcommerceInventory.Application.Features.Products.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommand : IRequest<ProductDto>
{
    public Guid    Id           { get; set; }
    public Guid    CategoryId   { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string? Description  { get; set; }
    public decimal UnitPrice    { get; set; }
    public decimal CostPrice    { get; set; }
    public int     ReorderLevel { get; set; }
    public int     ReorderQty   { get; set; }
    public string? Barcode      { get; set; }
    public decimal? WeightKg    { get; set; }
}
