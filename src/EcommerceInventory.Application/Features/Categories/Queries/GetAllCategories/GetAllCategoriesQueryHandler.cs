using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Categories.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Categories.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler
    : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllCategoriesQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<CategoryDto>> Handle(
        GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _uow.Categories.Query()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(c => c.IsActive);

        var categories = await query.ToListAsync(cancellationToken);

        var lookup = categories.ToDictionary(
            c => c.Id,
            c => new CategoryDto
            {
                Id          = c.Id,
                Name        = c.Name,
                Slug        = c.Slug,
                Description = c.Description,
                ImageUrl    = c.ImageUrl,
                ParentId    = c.ParentId,
                IsActive    = c.IsActive,
                SortOrder   = c.SortOrder,
                CreatedAt   = c.CreatedAt,
                UpdatedAt   = c.UpdatedAt
            });

        var roots = new List<CategoryDto>();

        foreach (var cat in categories)
        {
            var dto = lookup[cat.Id];
            if (cat.ParentId == null)
            {
                roots.Add(dto);
            }
            else if (lookup.TryGetValue(cat.ParentId.Value, out var parent))
            {
                dto.ParentName = parent.Name;
                parent.Children.Add(dto);
            }
        }

        return roots;
    }
}
