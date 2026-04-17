using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Categories.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICloudinaryService _cloudinary;

    public CreateCategoryCommandHandler(IUnitOfWork uow,
                                         ICloudinaryService cloudinary)
    {
        _uow       = uow;
        _cloudinary = cloudinary;
    }

    public async Task<CategoryDto> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ParentId.HasValue)
        {
            var parentExists = await _uow.Categories.ExistsAsync(
                request.ParentId.Value, cancellationToken);
            if (!parentExists)
                throw new NotFoundException("Parent category", request.ParentId.Value);
        }

        var slug = GenerateSlug(request.Name);

        var slugExists = await _uow.Categories.Query()
            .AnyAsync(c => c.Slug == slug, cancellationToken);
        if (slugExists)
            slug = slug + "-" + Guid.NewGuid().ToString("N")[..6];

        string? imageUrl    = null;
        string? cloudinaryId = null;

        if (request.ImageStream != null &&
            !string.IsNullOrEmpty(request.ImageFileName))
        {
            var result = await _cloudinary.UploadImageAsync(
                request.ImageStream,
                request.ImageFileName,
                request.ImageContentType ?? "image/jpeg",
                "categories");
            imageUrl     = result.SecureUrl;
            cloudinaryId = result.PublicId;
        }

        var category = Category.Create(
            request.Name, slug, request.Description, request.ParentId);

        if (imageUrl != null)
            category.SetImage(imageUrl, cloudinaryId!);

        category.SetSortOrder(request.SortOrder);

        await _uow.Categories.AddAsync(category, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new CategoryDto
        {
            Id          = category.Id,
            Name        = category.Name,
            Slug        = category.Slug,
            Description = category.Description,
            ImageUrl    = category.ImageUrl,
            ParentId    = category.ParentId,
            IsActive    = category.IsActive,
            SortOrder   = category.SortOrder,
            CreatedAt   = category.CreatedAt,
            UpdatedAt   = category.UpdatedAt
        };
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.Trim().ToLower()
            .Replace(" ", "-")
            .Replace("_", "-");

        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }
}
