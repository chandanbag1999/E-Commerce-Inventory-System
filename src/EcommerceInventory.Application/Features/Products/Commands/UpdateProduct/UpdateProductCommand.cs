using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Products.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand : IRequest<Result<ProductDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int ReorderLevel { get; set; }
    public int ReorderQty { get; set; }
    public decimal? WeightKg { get; set; }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required").MaximumLength(200);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IRepository<Product> _productRepository;

    public UpdateProductCommandHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _productRepository.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (product == null)
            return Result<ProductDto>.FailureResult("Product not found");

        product.Update(
            request.Name,
            request.UnitPrice,
            request.CostPrice,
            request.Description,
            request.ReorderLevel,
            request.ReorderQty,
            request.WeightKg
        );

        _productRepository.Update(product);

        var dto = new ProductDto(
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

        return Result<ProductDto>.SuccessResult(dto, "Product updated successfully");
    }
}