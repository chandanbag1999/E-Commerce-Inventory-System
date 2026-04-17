using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.AddSalesOrderItem;

public class AddSalesOrderItemCommandHandler
    : IRequestHandler<AddSalesOrderItemCommand, SalesOrderDto>
{
    private readonly IUnitOfWork _uow;

    public AddSalesOrderItemCommandHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<SalesOrderDto> Handle(
        AddSalesOrderItemCommand request,
        CancellationToken cancellationToken)
    {
        var so = await _uow.SalesOrders.Query()
            .Include(s => s.Items)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.SalesOrderId,
                                 cancellationToken);
        if (so == null)
            throw new NotFoundException("Sales order", request.SalesOrderId);

        var product = await _uow.Products.GetByIdAsync(
            request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException("Product", request.ProductId);

        so.AddItem(request.ProductId, request.Quantity,
                   request.UnitPrice, request.Discount);
        await _uow.SaveChangesAsync(cancellationToken);

        var productIds = so.Items.Select(i => i.ProductId).ToList();
        var productLookup = await _uow.Products.Query()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.Sku })
            .ToDictionaryAsync(p => p.Id, cancellationToken);

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
