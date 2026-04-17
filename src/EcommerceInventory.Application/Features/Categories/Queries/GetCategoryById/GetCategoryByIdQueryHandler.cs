using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Categories.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IUnitOfWork _uow;

    public GetCategoryByIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<CategoryDto> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _uow.Categories.Query()
            .Include(c => c.Children)
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
            throw new NotFoundException("Category", request.Id);

        return new CategoryDto
        {
            Id          = category.Id,
            Name        = category.Name,
            Slug        = category.Slug,
            Description = category.Description,
            ImageUrl    = category.ImageUrl,
            ParentId    = category.ParentId,
            ParentName  = category.Parent?.Name,
            IsActive    = category.IsActive,
            SortOrder   = category.SortOrder,
            CreatedAt   = category.CreatedAt,
            UpdatedAt   = category.UpdatedAt,
            Children    = category.Children.Select(c => new CategoryDto
            {
                Id        = c.Id,
                Name      = c.Name,
                Slug      = c.Slug,
                IsActive  = c.IsActive,
                SortOrder = c.SortOrder,
                ParentId  = c.ParentId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList()
        };
    }
}
