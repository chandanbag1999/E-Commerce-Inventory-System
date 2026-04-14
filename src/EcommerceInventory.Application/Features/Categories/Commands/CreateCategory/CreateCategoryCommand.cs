using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Categories.DTOs;
using EcommerceInventory.Domain.Entities;
using FluentValidation;
using MediatR;

namespace EcommerceInventory.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand : IRequest<Result<CategoryDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
}

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(150).WithMessage("Name cannot exceed 150 characters");
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IRepository<Category> _categoryRepository;

    public CreateCategoryCommandHandler(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var exists = await _categoryRepository.ExistsAsync(request.ParentId ?? Guid.Empty, ct);
        if (request.ParentId.HasValue && !exists)
            return Result<CategoryDto>.FailureResult("Parent category not found");

        var slug = GenerateSlug(request.Name);
        
        var category = Category.Create(
            request.Name,
            slug,
            request.Description,
            request.ParentId
        );

        await _categoryRepository.AddAsync(category, ct);

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

        return Result<CategoryDto>.SuccessResult(dto, "Category created successfully");
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.Trim().ToLower().Replace(" ", "-");
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return $"{slug}-{suffix}";
    }
}