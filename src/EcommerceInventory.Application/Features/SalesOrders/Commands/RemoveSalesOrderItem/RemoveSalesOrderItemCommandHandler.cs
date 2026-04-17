using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.RemoveSalesOrderItem;

public class RemoveSalesOrderItemCommandHandler
    : IRequestHandler<RemoveSalesOrderItemCommand, SalesOrderDto>
{
    private readonly IUnitOfWork _uow;

    public RemoveSalesOrderItemCommandHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<SalesOrderDto> Handle(
        RemoveSalesOrderItemCommand request,
        CancellationToken cancellationToken)
    {
        var so = await _uow.SalesOrders.Query()
            .Include(s => s.Items)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.SalesOrderId,
                                 cancellationToken);
        if (so == null)
            throw new NotFoundException("Sales order", request.SalesOrderId);

        so.RemoveItem(request.ItemId);
        await _uow.SaveChangesAsync(cancellationToken);

        var productIds = so.Items.Select(i => i.ProductId).ToList();
        var productLookup = new Dictionary<Guid, (string Name, string Sku)>();

        if (productIds.Any())
        {
            var products = await _uow.Products.Query()
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Name, p.Sku })
                .ToListAsync(cancellationToken);

            foreach (var p in products)
                productLookup[p.Id] = (p.Name, p.Sku);
        }

        return CreateSalesOrderCommandHandler.MapToDto(
            so, so.Warehouse.Name, null, null,
            so.Items.Select(i => new SalesOrderItemDto
            {
                Id          = i.Id,
                ProductId   = i.ProductId,
                ProductName = productLookup.ContainsKey(i.ProductId)
                                ? productLookup[i.ProductId].Name : string.Empty,
                ProductSku  = productLookup.ContainsKey(i.ProductId)
                                ? productLookup[i.ProductId].Sku : string.Empty,
                Quantity    = i.Quantity,
                UnitPrice   = i.UnitPrice,
                Discount    = i.Discount,
                LineTotal   = i.LineTotal
            }).ToList());
    }
}
