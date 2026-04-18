using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Enums;
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

        // Check for active stock
        var hasStock = await _uow.Stocks.Query()
            .AnyAsync(s => s.WarehouseId == request.Id && s.Quantity > 0,
                      cancellationToken);
        if (hasStock)
            throw new BusinessRuleViolationException(
                "Cannot delete warehouse that has active stock. " +
                "Please transfer or adjust stock first.");

        // #2: Check for pending purchase orders
        var hasPendingPO = await _uow.PurchaseOrders.Query()
            .AnyAsync(po => po.WarehouseId == request.Id
                && po.Status != OrderStatus.Received
                && po.Status != OrderStatus.Cancelled,
                cancellationToken);
        if (hasPendingPO)
            throw new BusinessRuleViolationException(
                "Cannot delete warehouse that has pending purchase orders. " +
                "Please complete or cancel them first.");

        // #2: Check for pending sales orders
        var hasPendingSO = await _uow.SalesOrders.Query()
            .AnyAsync(so => so.WarehouseId == request.Id
                && so.Status != OrderStatus.Delivered
                && so.Status != OrderStatus.Cancelled,
                cancellationToken);
        if (hasPendingSO)
            throw new BusinessRuleViolationException(
                "Cannot delete warehouse that has pending sales orders. " +
                "Please complete or cancel them first.");

        warehouse.SoftDelete();
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
