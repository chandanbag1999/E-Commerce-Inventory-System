using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.SubmitSalesOrder;

public record SubmitSalesOrderCommand(Guid SalesOrderId) : IRequest<Result<SalesOrderDto>>;

public class SubmitSalesOrderCommandHandler : IRequestHandler<SubmitSalesOrderCommand, Result<SalesOrderDto>>
{
    private readonly IRepository<SalesOrder> _salesOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitSalesOrderCommandHandler(
        IRepository<SalesOrder> salesOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _salesOrderRepository = salesOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SalesOrderDto>> Handle(SubmitSalesOrderCommand request, CancellationToken ct)
    {
        var salesOrder = await _salesOrderRepository.Query()
            .Include(s => s.Items)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.SalesOrderId, ct);

        if (salesOrder == null)
            return Result<SalesOrderDto>.FailureResult("Sales order not found");

        // Submit via domain method (validates Draft status + has items)
        salesOrder.Submit();

        await _unitOfWork.SaveChangesAsync(ct);

        var dto = MapToDto(salesOrder);

        return Result<SalesOrderDto>.SuccessResult(dto, "Sales order submitted");
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
