using MediatR;

namespace EcommerceInventory.Application.Features.Products.Commands.DeleteProductImage;

public class DeleteProductImageCommand : IRequest<bool>
{
    public Guid ProductId { get; set; }
    public Guid ImageId   { get; set; }
}
