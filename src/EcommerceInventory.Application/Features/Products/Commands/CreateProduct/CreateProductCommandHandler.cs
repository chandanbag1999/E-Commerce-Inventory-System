using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Products.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICloudinaryService _cloudinary;
    private readonly ICurrentUserService _currentUser;

    public CreateProductCommandHandler(IUnitOfWork uow,
                                        ICloudinaryService cloudinary,
                                        ICurrentUserService currentUser)
    {
        _uow         = uow;
        _cloudinary  = cloudinary;
        _currentUser = currentUser;
    }

    public async Task<ProductDto> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var skuExists = await _uow.Products.Query()
            .AnyAsync(p => p.Sku == request.Sku.ToUpper().Trim(),
                      cancellationToken);

        if (skuExists)
            throw new DomainException($"SKU '{request.Sku}' already exists.");

        var category = await _uow.Categories.GetByIdAsync(
            request.CategoryId, cancellationToken);

        if (category == null)
            throw new NotFoundException("Category", request.CategoryId);

        var slug = GenerateSlug(request.Name);
        var slugExists = await _uow.Products.Query()
            .AnyAsync(p => p.Slug == slug, cancellationToken);
        if (slugExists)
            slug = slug + "-" + Guid.NewGuid().ToString("N")[..6];

        var product = Product.Create(
            categoryId:   request.CategoryId,
            name:         request.Name,
            slug:         slug,
            sku:          request.Sku,
            unitPrice:    request.UnitPrice,
            costPrice:    request.CostPrice,
            description:  request.Description,
            reorderLevel: request.ReorderLevel,
            reorderQty:   request.ReorderQty,
            barcode:      request.Barcode,
            weightKg:     request.WeightKg,
            createdBy:    _currentUser.UserId);

        await _uow.Products.AddAsync(product, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        if (request.Images.Any())
        {
            var uploadTasks = request.Images.Select(img =>
                _cloudinary.UploadImageAsync(
                    img.FileStream, img.FileName, img.ContentType, "products"));

            var results = await Task.WhenAll(uploadTasks);

            for (int i = 0; i < results.Length; i++)
            {
                var image = ProductImage.Create(
                    productId:    product.Id,
                    cloudinaryId: results[i].PublicId,
                    imageUrl:     results[i].SecureUrl,
                    isPrimary:    i == 0,
                    displayOrder: i);

                await _uow.ProductImages.AddAsync(image, cancellationToken);
            }

            await _uow.SaveChangesAsync(cancellationToken);
        }

        return new ProductDto
        {
            Id           = product.Id,
            CategoryId   = product.CategoryId,
            CategoryName = category.Name,
            Name         = product.Name,
            Slug         = product.Slug,
            Description  = product.Description,
            Sku          = product.Sku,
            Barcode      = product.Barcode,
            UnitPrice    = product.UnitPrice,
            CostPrice    = product.CostPrice,
            ReorderLevel = product.ReorderLevel,
            ReorderQty   = product.ReorderQty,
            Status       = product.Status.ToString(),
            WeightKg     = product.WeightKg,
            CreatedAt    = product.CreatedAt,
            UpdatedAt    = product.UpdatedAt
        };
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.Trim().ToLower().Replace(" ", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }
}
