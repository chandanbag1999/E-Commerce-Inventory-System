using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Queries.GetSalesOrderById;

public record GetSalesOrderByIdQuery(Guid Id) : IRequest<Result<SalesOrderDto>>;

public class GetSalesOrderByIdQueryHandler : IRequestHandler<GetSalesOrderByIdQuery, Result<SalesOrderDto>>
{
    private readonly IRepository<SalesOrder> _salesOrderRepository;

    public GetSalesOrderByIdQueryHandler(IRepository<SalesOrder> salesOrderRepository)
    {
        _salesOrderRepository = salesOrderRepository;
    }

    public async Task<Result<SalesOrderDto>> Handle(GetSalesOrderByIdQuery request, CancellationToken ct)
    {
        var salesOrder = await _salesOrderRepository.Query()
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (salesOrder == null)
            return Result<SalesOrderDto>.FailureResult("Sales order not found");

        var dto = MapToDto(salesOrder);

        return Result<SalesOrderDto>.SuccessResult(dto);
    }

    private static SalesOrderDto MapToDto(SalesOrder so)
    {
        return new SalesOrderDto(
            so.Id,
            so.SoNumber,
            so.CustomerName,
            so.CustomerEmail,
            so.CustomerPhone,
            so.WarehouseId,
            so.Warehouse?.Name ?? string.Empty,
            so.Status.ToString(),
            so.Subtotal,
            so.TotalAmount,
            so.Notes,
            so.ShippingAddressJson,
            so.ApprovedBy,
            so.ApprovedAt,
            so.ShippedAt,
            so.DeliveredAt,
            so.Items.Select(i => new SalesOrderItemDto(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? string.Empty,
                i.Product?.Sku ?? string.Empty,
                i.Quantity,
                i.UnitPrice,
                i.Discount,
                i.Total
            )).ToList(),
            so.CreatedAt,
            so.UpdatedAt
        );
    }
}
