using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;

namespace EcommerceInventory.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler
    : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteProductCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _uow.Products.GetByIdAsync(
            request.Id, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product", request.Id);

        product.SoftDelete();
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
