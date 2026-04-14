using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Categories.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IRequest<Result<CategoryDto>>;

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly IRepository<Category> _categoryRepository;

    public GetCategoryByIdHandler(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken ct)
    {
        var category = await _categoryRepository.Query()
            .Include(c => c.Children.Where(child => child.IsActive))
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct);

        if (category == null)
            return Result<CategoryDto>.FailureResult("Category not found");

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
            category.Children.Select(c => new CategoryDto(
                c.Id, c.Name, c.Slug, c.Description, c.ImageUrl, c.ParentId, 
                c.IsActive, c.SortOrder, c.CreatedAt, new List<CategoryDto>()
            )).ToList()
        );

        return Result<CategoryDto>.SuccessResult(dto);
    }
}