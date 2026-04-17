using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Queries.GetSalesOrderById;

public class GetSalesOrderByIdQueryHandler
    : IRequestHandler<GetSalesOrderByIdQuery, SalesOrderDto>
{
    private readonly IUnitOfWork _uow;

    public GetSalesOrderByIdQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<SalesOrderDto> Handle(
        GetSalesOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var so = await _uow.SalesOrders.Query()
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (so == null)
            throw new NotFoundException("Sales order", request.Id);

        string? createdByName  = null;
        string? approvedByName = null;

        var creator = await _uow.Users.GetByIdAsync(
            so.CreatedBy, cancellationToken);
        createdByName = creator?.FullName;

        if (so.ApprovedBy.HasValue)
        {
            var approver = await _uow.Users.GetByIdAsync(
                so.ApprovedBy.Value, cancellationToken);
            approvedByName = approver?.FullName;
        }

        return CreateSalesOrderCommandHandler.MapToDto(
            so, so.Warehouse.Name, createdByName, approvedByName,
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
