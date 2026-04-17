using EcommerceInventory.Application.Features.Categories.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommand : IRequest<CategoryDto>
{
    public Guid    Id          { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid?   ParentId    { get; set; }
    public int     SortOrder   { get; set; } = 0;
}
