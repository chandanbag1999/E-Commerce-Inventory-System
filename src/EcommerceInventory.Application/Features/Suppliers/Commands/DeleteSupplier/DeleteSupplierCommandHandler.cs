using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Suppliers.Commands.DeleteSupplier;

public class DeleteSupplierCommandHandler
    : IRequestHandler<DeleteSupplierCommand>
{
    private readonly IUnitOfWork _uow;

    public DeleteSupplierCommandHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task Handle(
        DeleteSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(
            request.Id, cancellationToken);
        if (supplier == null)
            throw new NotFoundException("Supplier", request.Id);

        // Check active purchase orders
        var hasActiveOrders = await _uow.PurchaseOrders.Query()
            .AnyAsync(po => po.SupplierId == request.Id &&
                            po.Status != OrderStatus.Received &&
                            po.Status != OrderStatus.Cancelled &&
                            po.Status != OrderStatus.Rejected,
                      cancellationToken);

        if (hasActiveOrders)
            throw new BusinessRuleViolationException(
                "Cannot delete supplier with active purchase orders.");

        supplier.SoftDelete();
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
