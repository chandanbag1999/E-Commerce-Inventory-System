using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Warehouses.Commands.DeleteWarehouse;

public class DeleteWarehouseCommandHandler
    : IRequestHandler<DeleteWarehouseCommand>
{
    private readonly IUnitOfWork _uow;

    public DeleteWarehouseCommandHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task Handle(
        DeleteWarehouseCommand request,
        CancellationToken cancellationToken)
    {
        var warehouse = await _uow.Warehouses.GetByIdAsync(
            request.Id, cancellationToken);
        if (warehouse == null)
            throw new NotFoundException("Warehouse", request.Id);

        // Check if warehouse has active stock
        var hasStock = await _uow.Stocks.Query()
            .AnyAsync(s => s.WarehouseId == request.Id && s.Quantity > 0,
                      cancellationToken);
        if (hasStock)
            throw new BusinessRuleViolationException(
                "Cannot delete warehouse that has active stock. " +
                "Please transfer or adjust stock first.");

        warehouse.SoftDelete();
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
