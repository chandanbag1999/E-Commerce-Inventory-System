using MediatR;

namespace EcommerceInventory.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
