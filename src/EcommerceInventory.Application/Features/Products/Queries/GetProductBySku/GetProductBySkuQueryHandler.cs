using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Products.DTOs;
using EcommerceInventory.Application.Features.Products.Queries.GetProductById;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Products.Queries.GetProductBySku;

public class GetProductBySkuQueryHandler
    : IRequestHandler<GetProductBySkuQuery, ProductDto>
{
    private readonly IUnitOfWork _uow;

    public GetProductBySkuQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ProductDto> Handle(
        GetProductBySkuQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _uow.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.Stocks)
            .FirstOrDefaultAsync(
                p => p.Sku == request.Sku.ToUpper().Trim(),
                cancellationToken);

        if (product == null)
            throw new NotFoundException($"Product with SKU '{request.Sku}' not found.");

        return GetProductByIdQueryHandler.MapToDto(product);
    }
}
