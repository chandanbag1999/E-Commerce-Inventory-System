using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.RemovePurchaseOrderItem;

public class RemovePurchaseOrderItemCommandHandler
    : IRequestHandler<RemovePurchaseOrderItemCommand, PurchaseOrderDto>
{
    private readonly IUnitOfWork _uow;

    public RemovePurchaseOrderItemCommandHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<PurchaseOrderDto> Handle(
        RemovePurchaseOrderItemCommand request,
        CancellationToken cancellationToken)
    {
        var po = await _uow.PurchaseOrders.Query()
            .Include(p => p.Items)
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId,
                                 cancellationToken);
        if (po == null)
            throw new NotFoundException("Purchase order", request.PurchaseOrderId);

        po.RemoveItem(request.ItemId);
        await _uow.SaveChangesAsync(cancellationToken);

        var productIds = po.Items.Select(i => i.ProductId).ToList();
        var productLookup = await _uow.Products.Query()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.Sku })
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var items = po.Items.Select(i => new PurchaseOrderItemDto
        {
            Id               = i.Id,
            ProductId        = i.ProductId,
            ProductName      = productLookup.ContainsKey(i.ProductId)
                                ? productLookup[i.ProductId].Name : string.Empty,
            ProductSku       = productLookup.ContainsKey(i.ProductId)
                                ? productLookup[i.ProductId].Sku : string.Empty,
            QuantityOrdered  = i.QuantityOrdered,
            QuantityReceived = i.QuantityReceived,
            UnitCost         = i.UnitCost,
            TotalCost        = i.TotalCost
        }).ToList();

        return CreatePurchaseOrderCommandHandler.MapToDto(
            po, po.Supplier.Name, po.Warehouse.Name, null, null, items);
    }
}
