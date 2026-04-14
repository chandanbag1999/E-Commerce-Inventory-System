using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Categories.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace EcommerceInventory.Application.Features.Categories.Commands.UploadCategoryImage;

public record UploadCategoryImageCommand : IRequest<Result<CategoryDto>>
{
    public Guid CategoryId { get; set; }
    public IFormFile ImageFile { get; set; } = null!;
}

public class UploadCategoryImageCommandValidator : AbstractValidator<UploadCategoryImageCommand>
{
    public UploadCategoryImageCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required");

        RuleFor(x => x.ImageFile)
            .NotNull().WithMessage("Image file is required")
            .Must(file => file.Length > 0).WithMessage("File cannot be empty")
            .Must(file =>
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                return allowedTypes.Contains(file.ContentType.ToLower());
            }).WithMessage("Only JPEG, PNG, and WebP formats are allowed")
            .Must(file => file.Length <= 5 * 1024 * 1024).WithMessage("Maximum file size is 5MB");
    }
}

public class UploadCategoryImageCommandHandler : IRequestHandler<UploadCategoryImageCommand, Result<CategoryDto>>
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly ICloudinaryService _cloudinaryService;

    public UploadCategoryImageCommandHandler(
        IRepository<Category> categoryRepository,
        ICloudinaryService cloudinaryService)
    {
        _categoryRepository = categoryRepository;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<CategoryDto>> Handle(UploadCategoryImageCommand request, CancellationToken ct)
    {
        // 1. Find category
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, ct);
        if (category == null)
            throw new NotFoundException("Category", request.CategoryId);

        // 2. Upload new image to Cloudinary
        var (secureUrl, publicId) = await _cloudinaryService.UploadImageAsync(request.ImageFile, "categories");

        // 3. Delete old image from Cloudinary if exists
        if (!string.IsNullOrEmpty(category.CloudinaryId))
        {
            try
            {
                await _cloudinaryService.DeleteImageAsync(category.CloudinaryId);
            }
            catch
            {
                // Log but don't fail - old image deletion failure shouldn't block new upload
            }
        }

        // 4. Update category with new image
        category.SetImage(secureUrl, publicId);
        _categoryRepository.Update(category);

        // 5. Return updated category DTO
        var dto = new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ImageUrl,
            category.ParentId,
            category.IsActive,
            category.SortOrder,
            category.CreatedAt,
            new List<CategoryDto>()
        );

        return Result<CategoryDto>.SuccessResult(dto, "Category image uploaded successfully");
    }
}
