using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Products.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IUnitOfWork _uow;

    public GetProductByIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ProductDto> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _uow.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.Stocks)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product", request.Id);

        return MapToDto(product);
    }

    public static ProductDto MapToDto(Domain.Entities.Product p)
    {
        return new ProductDto
        {
            Id           = p.Id,
            CategoryId   = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty,
            Name         = p.Name,
            Slug         = p.Slug,
            Description  = p.Description,
            Sku          = p.Sku,
            Barcode      = p.Barcode,
            UnitPrice    = p.UnitPrice,
            CostPrice    = p.CostPrice,
            ReorderLevel = p.ReorderLevel,
            ReorderQty   = p.ReorderQty,
            Status       = p.Status.ToString(),
            WeightKg     = p.WeightKg,
            PrimaryImage = p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                        ?? p.Images.FirstOrDefault()?.ImageUrl,
            Images       = p.Images.Select(i => new ProductImageDto
            {
                Id           = i.Id,
                ImageUrl     = i.ImageUrl,
                IsPrimary    = i.IsPrimary,
                DisplayOrder = i.DisplayOrder
            }).ToList(),
            TotalStock   = p.Stocks.Sum(s => s.Quantity),
            CreatedAt    = p.CreatedAt,
            UpdatedAt    = p.UpdatedAt
        };
    }
}
