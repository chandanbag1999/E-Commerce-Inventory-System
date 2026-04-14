using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Products.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IRepository<Product> _productRepository;

    public GetProductByIdHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await _productRepository.Query()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (product == null)
            return Result<ProductDto>.FailureResult("Product not found");

        var dto = MapToDto(product);
        return Result<ProductDto>.SuccessResult(dto);
    }

    private static ProductDto MapToDto(Product product) => new(
        product.Id,
        product.CategoryId,
        product.Category.Name,
        product.Name,
        product.Slug,
        product.Sku,
        product.Description,
        product.Barcode,
        product.UnitPrice,
        product.CostPrice,
        product.ReorderLevel,
        product.ReorderQty,
        product.Status.ToString(),
        product.WeightKg,
        product.CreatedAt,
        product.Images.Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.IsPrimary, i.DisplayOrder)).ToList()
    );
}