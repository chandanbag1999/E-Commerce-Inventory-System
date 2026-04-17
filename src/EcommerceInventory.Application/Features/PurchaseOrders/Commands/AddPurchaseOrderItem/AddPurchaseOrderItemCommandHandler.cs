using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.AddPurchaseOrderItem;

public class AddPurchaseOrderItemCommandHandler
    : IRequestHandler<AddPurchaseOrderItemCommand, PurchaseOrderDto>
{
    private readonly IUnitOfWork _uow;

    public AddPurchaseOrderItemCommandHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<PurchaseOrderDto> Handle(
        AddPurchaseOrderItemCommand request,
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

        var product = await _uow.Products.GetByIdAsync(
            request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException("Product", request.ProductId);

        po.AddItem(request.ProductId, request.QuantityOrdered, request.UnitCost);
        await _uow.SaveChangesAsync(cancellationToken);

        var productLookup = await _uow.Products.Query()
            .Where(p => po.Items.Select(i => i.ProductId).Contains(p.Id))
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
