using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Products.DTOs;
using EcommerceInventory.Application.Features.Products.Queries.GetProductById;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateProductCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ProductDto> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _uow.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Stocks)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product", request.Id);

        var category = await _uow.Categories.GetByIdAsync(
            request.CategoryId, cancellationToken);

        if (category == null)
            throw new NotFoundException("Category", request.CategoryId);

        product.Update(request.CategoryId, request.Name, request.Description,
                       request.UnitPrice, request.CostPrice,
                       request.ReorderLevel, request.ReorderQty,
                       request.Barcode, request.WeightKg);

        await _uow.SaveChangesAsync(cancellationToken);

        return GetProductByIdQueryHandler.MapToDto(product);
    }
}
