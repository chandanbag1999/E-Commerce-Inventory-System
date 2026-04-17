using MediatR;

namespace EcommerceInventory.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
