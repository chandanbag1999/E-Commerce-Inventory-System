using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Products.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace EcommerceInventory.Application.Features.Products.Commands.DeleteProductImage;

public record DeleteProductImageCommand : IRequest<Result<ProductDto>>
{
    public Guid ProductId { get; set; }
    public Guid ImageId { get; set; }
}

public class DeleteProductImageCommandValidator : AbstractValidator<DeleteProductImageCommand>
{
    public DeleteProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.ImageId)
            .NotEmpty().WithMessage("Image ID is required");
    }
}

public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, Result<ProductDto>>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductImage> _productImageRepository;
    private readonly ICloudinaryService _cloudinaryService;

    public DeleteProductImageCommandHandler(
        IRepository<Product> productRepository,
        IRepository<ProductImage> productImageRepository,
        ICloudinaryService cloudinaryService)
    {
        _productRepository = productRepository;
        _productImageRepository = productImageRepository;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<ProductDto>> Handle(DeleteProductImageCommand request, CancellationToken ct)
    {
        // 1. Find product image
        var productImage = await _productImageRepository.GetByIdAsync(request.ImageId, ct);
        if (productImage == null || productImage.ProductId != request.ProductId)
            throw new NotFoundException("Product image", request.ImageId);

        // 2. Delete image from Cloudinary
        if (!string.IsNullOrEmpty(productImage.CloudinaryId))
        {
            try
            {
                await _cloudinaryService.DeleteImageAsync(productImage.CloudinaryId);
            }
            catch
            {
                // Log but don't fail - Cloudinary deletion failure shouldn't block DB deletion
            }
        }

        // 3. Delete from database
        _productImageRepository.Delete(productImage);

        // 4. If this was the primary image, set another image as primary
        if (productImage.IsPrimary)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId, ct);
            var nextImage = product?.Images
                .Where(i => i.Id != request.ImageId)
                .OrderBy(i => i.DisplayOrder)
                .FirstOrDefault();

            if (nextImage != null)
            {
                nextImage.IsPrimary = true;
                _productImageRepository.Update(nextImage);
            }
        }

        // 5. Reload product with updated images
        var updatedProduct = await _productRepository.GetByIdAsync(request.ProductId, ct);

        // 6. Return updated product DTO
        var dto = MapToDto(updatedProduct!);
        return Result<ProductDto>.SuccessResult(dto, "Product image deleted successfully");
    }

    private static ProductDto MapToDto(Product product)
    {
        var imageDtos = product.Images
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.IsPrimary, i.DisplayOrder))
            .ToList();

        return new ProductDto(
            product.Id,
            product.CategoryId,
            product.Category?.Name ?? "",
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
            imageDtos
        );
    }
}
