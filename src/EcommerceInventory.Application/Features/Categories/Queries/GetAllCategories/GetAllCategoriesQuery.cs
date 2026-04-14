using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Categories.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Categories.Queries.GetAllCategories;

public record GetAllCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;

public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    private readonly IRepository<Category> _categoryRepository;

    public GetAllCategoriesHandler(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken ct)
    {
        var categories = await _categoryRepository.Query()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

        var dtos = BuildCategoryTree(categories);
        return Result<IReadOnlyList<CategoryDto>>.SuccessResult(dtos);
    }

    class TreeNode
    {
        public CategoryDto Dto { get; set; } = null!;
        public List<TreeNode> Children { get; set; } = new();
    }

    private IReadOnlyList<CategoryDto> BuildCategoryTree(List<Category> flatCategories)
    {
        var nodeMap = new Dictionary<Guid, TreeNode>();
        
        foreach (var category in flatCategories)
        {
            nodeMap[category.Id] = new TreeNode
            {
                Dto = new CategoryDto(
                    category.Id,
                    category.Name,
                    category.Slug,
                    category.Description,
                    category.ImageUrl,
                    category.ParentId,
                    category.IsActive,
                    category.SortOrder,
                    category.CreatedAt,
                    Array.Empty<CategoryDto>()
                )
            };
        }

        var roots = new List<TreeNode>();

        // Build tree
        foreach (var category in flatCategories)
        {
            var node = nodeMap[category.Id];
            if (category.ParentId == null)
            {
                roots.Add(node);
            }
            else if (nodeMap.TryGetValue(category.ParentId.Value, out var parentNode))
            {
                parentNode.Children.Add(node);
            }
        }

        // Convert to DTOs with proper children
        return BuildDtoTree(roots);
    }

    private List<CategoryDto> BuildDtoTree(List<TreeNode> nodes)
    {
        var result = new List<CategoryDto>();
        foreach (var node in nodes)
        {
            var children = BuildDtoTree(node.Children);
            var dto = node.Dto with { Children = children };
            result.Add(dto);
        }
        return result;
    }
}