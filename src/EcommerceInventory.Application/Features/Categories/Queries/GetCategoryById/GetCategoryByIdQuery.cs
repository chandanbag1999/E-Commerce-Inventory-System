using EcommerceInventory.Application.Features.Categories.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQuery : IRequest<CategoryDto>
{
    public Guid Id { get; set; }
}
