using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.CancelSalesOrder;

public class CancelSalesOrderCommandHandler
    : IRequestHandler<CancelSalesOrderCommand, SalesOrderDto>
{
    private readonly IUnitOfWork _uow;

    public CancelSalesOrderCommandHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<SalesOrderDto> Handle(
        CancelSalesOrderCommand request,
        CancellationToken cancellationToken)
    {
        var so = await _uow.SalesOrders.Query()
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (so == null)
            throw new NotFoundException("Sales order", request.Id);

        await _uow.BeginTransactionAsync(cancellationToken);
        try
        {
            // If approved → reservations were made → release them
            if (so.Status == OrderStatus.Approved)
            {
                foreach (var item in so.Items)
                {
                    var stock = await _uow.Stocks.Query()
                        .FirstOrDefaultAsync(
                            s => s.ProductId   == item.ProductId &&
                                 s.WarehouseId == so.WarehouseId,
                            cancellationToken);

                    if (stock != null && stock.ReservedQty >= item.Quantity)
                        stock.ReleaseReservation(item.Quantity);
                }
            }

            so.Cancel();
            await _uow.SaveChangesAsync(cancellationToken);
            await _uow.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return CreateSalesOrderCommandHandler.MapToDto(
            so, so.Warehouse.Name, null, null,
            so.Items.Select(i => new SalesOrderItemDto
            {
                Id          = i.Id,
                ProductId   = i.ProductId,
                ProductName = i.Product.Name,
                ProductSku  = i.Product.Sku,
                Quantity    = i.Quantity,
                UnitPrice   = i.UnitPrice,
                Discount    = i.Discount,
                LineTotal   = i.LineTotal
            }).ToList());
    }
}
