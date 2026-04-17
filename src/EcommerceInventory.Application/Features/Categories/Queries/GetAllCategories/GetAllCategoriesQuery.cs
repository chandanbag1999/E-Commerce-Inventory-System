using EcommerceInventory.Application.Features.Categories.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Categories.Queries.GetAllCategories;

public class GetAllCategoriesQuery : IRequest<List<CategoryDto>>
{
    public bool IncludeInactive { get; set; } = false;
}
