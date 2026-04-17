using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;

public class CancelPurchaseOrderCommandHandler
    : IRequestHandler<CancelPurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IUnitOfWork _uow;

    public CancelPurchaseOrderCommandHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<PurchaseOrderDto> Handle(
        CancelPurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        var po = await _uow.PurchaseOrders.Query()
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (po == null)
            throw new NotFoundException("Purchase order", request.Id);

        po.Cancel();
        await _uow.SaveChangesAsync(cancellationToken);

        return CreatePurchaseOrderCommandHandler.MapToDto(
            po, po.Supplier.Name, po.Warehouse.Name, null, null,
            po.Items.Select(i => new PurchaseOrderItemDto
            {
                Id               = i.Id,
                ProductId        = i.ProductId,
                ProductName      = i.Product.Name,
                ProductSku       = i.Product.Sku,
                QuantityOrdered  = i.QuantityOrdered,
                QuantityReceived = i.QuantityReceived,
                UnitCost         = i.UnitCost,
                TotalCost        = i.TotalCost
            }).ToList());
    }
}
