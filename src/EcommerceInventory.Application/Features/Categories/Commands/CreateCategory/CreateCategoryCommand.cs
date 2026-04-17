using EcommerceInventory.Application.Features.Categories.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommand : IRequest<CategoryDto>
{
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid?   ParentId    { get; set; }
    public int     SortOrder   { get; set; } = 0;
    public Stream? ImageStream  { get; set; }
    public string? ImageFileName { get; set; }
    public string? ImageContentType { get; set; }
}
