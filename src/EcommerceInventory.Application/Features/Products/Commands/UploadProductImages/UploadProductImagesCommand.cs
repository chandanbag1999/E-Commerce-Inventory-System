using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Products.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace EcommerceInventory.Application.Features.Products.Commands.UploadProductImages;

public record UploadProductImagesCommand : IRequest<Result<ProductDto>>
{
    public Guid ProductId { get; set; }
    public List<IFormFile> Images { get; set; } = new();
}

public class UploadProductImagesCommandValidator : AbstractValidator<UploadProductImagesCommand>
{
    public UploadProductImagesCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Images)
            .NotNull().WithMessage("Images are required")
            .Must(images => images.Count > 0 && images.Count <= 10)
            .WithMessage("You can upload between 1 and 10 images");

        RuleForEach(x => x.Images)
            .Must(file => file.Length > 0).WithMessage("File cannot be empty")
            .Must(file =>
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                return allowedTypes.Contains(file.ContentType.ToLower());
            }).WithMessage("Only JPEG, PNG, and WebP formats are allowed")
            .Must(file => file.Length <= 5 * 1024 * 1024).WithMessage("Maximum file size is 5MB per image");
    }
}

public class UploadProductImagesCommandHandler : IRequestHandler<UploadProductImagesCommand, Result<ProductDto>>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductImage> _productImageRepository;
    private readonly ICloudinaryService _cloudinaryService;

    public UploadProductImagesCommandHandler(
        IRepository<Product> productRepository,
        IRepository<ProductImage> productImageRepository,
        ICloudinaryService cloudinaryService)
    {
        _productRepository = productRepository;
        _productImageRepository = productImageRepository;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<ProductDto>> Handle(UploadProductImagesCommand request, CancellationToken ct)
    {
        // 1. Find product
        var product = await _productRepository.GetByIdAsync(request.ProductId, ct);
        if (product == null)
            throw new NotFoundException("Product", request.ProductId);

        // 2. Check if product already has images (to determine display order)
        var existingImagesCount = product.Images.Count;

        // 3. Upload images to Cloudinary in parallel
        var uploadTasks = request.Images.Select(img => 
            _cloudinaryService.UploadImageAsync(img, "products"));
        var results = await Task.WhenAll(uploadTasks);

        // 4. Create ProductImage entities
        var productImages = new List<ProductImage>();
        for (var i = 0; i < results.Length; i++)
        {
            var (secureUrl, publicId) = results[i];
            var productImage = ProductImage.Create(
                productId: product.Id,
                cloudinaryId: publicId,
                imageUrl: secureUrl,
                isPrimary: existingImagesCount == 0 && i == 0, // First image is primary if no existing images
                displayOrder: existingImagesCount + i
            );
            productImages.Add(productImage);
        }

        // 5. Add images to product
        foreach (var productImage in productImages)
        {
            await _productImageRepository.AddAsync(productImage, ct);
        }

        // 6. Reload product with images
        product = await _productRepository.GetByIdAsync(request.ProductId, ct);

        // 7. Return updated product DTO
        var dto = MapToDto(product!);
        return Result<ProductDto>.SuccessResult(dto, "Product images uploaded successfully");
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
