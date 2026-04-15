using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.RemoveSalesOrderItem;

public record RemoveSalesOrderItemCommand(Guid SalesOrderId, Guid ItemId) : IRequest<Result<SalesOrderDto>>;

public class RemoveSalesOrderItemCommandHandler : IRequestHandler<RemoveSalesOrderItemCommand, Result<SalesOrderDto>>
{
    private readonly IRepository<SalesOrder> _salesOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveSalesOrderItemCommandHandler(
        IRepository<SalesOrder> salesOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _salesOrderRepository = salesOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SalesOrderDto>> Handle(RemoveSalesOrderItemCommand request, CancellationToken ct)
    {
        var salesOrder = await _salesOrderRepository.Query()
            .Include(s => s.Items)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.SalesOrderId, ct);

        if (salesOrder == null)
            return Result<SalesOrderDto>.FailureResult("Sales order not found");

        // Remove item via domain method (validates Draft status)
        salesOrder.RemoveItem(request.ItemId);

        await _unitOfWork.SaveChangesAsync(ct);

        var dto = MapToDto(salesOrder);

        return Result<SalesOrderDto>.SuccessResult(dto, "Item removed from sales order");
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
