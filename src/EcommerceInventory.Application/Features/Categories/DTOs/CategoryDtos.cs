namespace EcommerceInventory.Application.Features.Categories.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    Guid? ParentId,
    bool IsActive,
    int SortOrder,
    DateTime CreatedAt,
    IReadOnlyList<CategoryDto> Children
);

public record CategoryListDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    Guid? ParentId,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateCategoryDto(
    string Name,
    string? Description = null,
    Guid? ParentId = null
);

public record UpdateCategoryDto(
    string Name,
    string? Description = null
);