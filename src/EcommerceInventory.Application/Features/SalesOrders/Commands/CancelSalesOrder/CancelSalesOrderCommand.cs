using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.CancelSalesOrder;

public record CancelSalesOrderCommand(Guid SalesOrderId) : IRequest<Result<SalesOrderDto>>;

public class CancelSalesOrderCommandHandler : IRequestHandler<CancelSalesOrderCommand, Result<SalesOrderDto>>
{
    private readonly IRepository<SalesOrder> _salesOrderRepository;
    private readonly IRepository<Stock> _stockRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelSalesOrderCommandHandler(
        IRepository<SalesOrder> salesOrderRepository,
        IRepository<Stock> stockRepository,
        IUnitOfWork unitOfWork)
    {
        _salesOrderRepository = salesOrderRepository;
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SalesOrderDto>> Handle(CancelSalesOrderCommand request, CancellationToken ct)
    {
        var salesOrder = await _salesOrderRepository.Query()
            .Include(s => s.Items)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.SalesOrderId, ct);

        if (salesOrder == null)
            return Result<SalesOrderDto>.FailureResult("Sales order not found");

        // Cannot cancel if already shipped or delivered
        if (salesOrder.Status == OrderStatus.Shipped || salesOrder.Status == OrderStatus.Delivered)
            return Result<SalesOrderDto>.FailureResult("Cannot cancel shipped or delivered orders");

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // If status is Approved (stock was reserved), release all reservations
            if (salesOrder.Status == OrderStatus.Approved)
            {
                foreach (var item in salesOrder.Items)
                {
                    var stock = await _stockRepository.Query()
                        .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == salesOrder.WarehouseId, ct);

                    if (stock != null)
                    {
                        stock.ReleaseReservation(item.Quantity);
                    }
                }
            }

            // Cancel the sales order
            salesOrder.Cancel();

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            // Reload with navigation properties
            var updatedSo = await _salesOrderRepository.Query()
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .Include(s => s.Warehouse)
                .FirstOrDefaultAsync(s => s.Id == salesOrder.Id, ct);

            var dto = MapToDto(updatedSo!);

            return Result<SalesOrderDto>.SuccessResult(dto, "Sales order cancelled");
        }
        catch (BusinessRuleViolationException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            return Result<SalesOrderDto>.FailureResult(ex.Message);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
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
