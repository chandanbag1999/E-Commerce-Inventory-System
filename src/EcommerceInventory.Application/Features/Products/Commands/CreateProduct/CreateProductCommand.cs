using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Products.DTOs;
using EcommerceInventory.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand : IRequest<Result<ProductDto>>
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int ReorderLevel { get; set; }
    public int ReorderQty { get; set; }
    public string? Barcode { get; set; }
    public decimal? WeightKg { get; set; }
    public List<IFormFile>? Images { get; set; }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required").MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().WithMessage("SKU is required").MaximumLength(100);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Unit price must be >= 0");
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0).WithMessage("Cost price must be >= 0");
        
        RuleFor(x => x.Images)
            .Must(images => images == null || images.Count <= 10)
            .WithMessage("Maximum 10 images allowed");
            
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

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductImage> _productImageRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly ICloudinaryService _cloudinaryService;

    public CreateProductCommandHandler(
        IRepository<Product> productRepository,
        IRepository<ProductImage> productImageRepository,
        IRepository<Category> categoryRepository,
        ICloudinaryService cloudinaryService)
    {
        _productRepository = productRepository;
        _productImageRepository = productImageRepository;
        _categoryRepository = categoryRepository;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId, ct);
        if (!categoryExists)
            return Result<ProductDto>.FailureResult("Category not found");

        var skuExists = await _productRepository.Query().AnyAsync(p => p.Sku == request.Sku.ToUpper(), ct);
        if (skuExists)
            return Result<ProductDto>.FailureResult("SKU already exists");

        var product = Product.Create(
            request.CategoryId,
            request.Name,
            request.Sku,
            request.UnitPrice,
            request.CostPrice,
            request.Description,
            request.ReorderLevel,
            request.ReorderQty
        );

        product.Barcode = request.Barcode;
        product.WeightKg = request.WeightKg;

        // Upload images if provided
        var imageDtos = new List<ProductImageDto>();
        if (request.Images != null && request.Images.Any())
        {
            // Upload images to Cloudinary in parallel
            var uploadTasks = request.Images.Select(img => 
                _cloudinaryService.UploadImageAsync(img, "products"));
            var results = await Task.WhenAll(uploadTasks);

            // Create ProductImage entities
            for (var i = 0; i < results.Length; i++)
            {
                var (secureUrl, publicId) = results[i];
                var productImage = ProductImage.Create(
                    productId: product.Id,
                    cloudinaryId: publicId,
                    imageUrl: secureUrl,
                    isPrimary: i == 0, // First image is primary
                    displayOrder: i
                );
                await _productImageRepository.AddAsync(productImage, ct);
                
                imageDtos.Add(new ProductImageDto(productImage.Id, productImage.ImageUrl, productImage.IsPrimary, productImage.DisplayOrder));
            }
        }

        await _productRepository.AddAsync(product, ct);

        var dto = new ProductDto(
            product.Id,
            product.CategoryId,
            "",
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

        return Result<ProductDto>.SuccessResult(dto, "Product created successfully");
    }
}