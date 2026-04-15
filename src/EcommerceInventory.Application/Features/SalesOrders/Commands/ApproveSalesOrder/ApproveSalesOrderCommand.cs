using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.ApproveSalesOrder;

public record ApproveSalesOrderCommand(Guid SalesOrderId) : IRequest<Result<SalesOrderDto>>;

public class ApproveSalesOrderCommandHandler : IRequestHandler<ApproveSalesOrderCommand, Result<SalesOrderDto>>
{
    private readonly IRepository<SalesOrder> _salesOrderRepository;
    private readonly IRepository<Stock> _stockRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ApproveSalesOrderCommandHandler(
        IRepository<SalesOrder> salesOrderRepository,
        IRepository<Stock> stockRepository,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _salesOrderRepository = salesOrderRepository;
        _stockRepository = stockRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SalesOrderDto>> Handle(ApproveSalesOrderCommand request, CancellationToken ct)
    {
        // Load SO with items
        var salesOrder = await _salesOrderRepository.Query()
            .Include(s => s.Items)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.SalesOrderId, ct);

        if (salesOrder == null)
            return Result<SalesOrderDto>.FailureResult("Sales order not found");

        if (salesOrder.Status != OrderStatus.Submitted)
            return Result<SalesOrderDto>.FailureResult("Only Submitted orders can be approved");

        // 4-eye principle: creator cannot approve their own order
        var currentUserId = _currentUserService.UserId ?? Guid.Empty;
        if (salesOrder.CreatedBy == currentUserId)
            return Result<SalesOrderDto>.FailureResult(
                "You cannot approve an order you created (4-eye principle)");

        // PRE-CHECK: Validate stock for ALL items BEFORE any reservation
        foreach (var item in salesOrder.Items)
        {
            var stock = await _stockRepository.Query()
                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == salesOrder.WarehouseId, ct);

            if (stock == null || stock.AvailableQty < item.Quantity)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
                var available = stock?.AvailableQty ?? 0;
                return Result<SalesOrderDto>.FailureResult(
                    $"Insufficient stock for '{product?.Name ?? "Unknown Product"}'. " +
                    $"Available: {available}, Required: {item.Quantity}");
            }
        }

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Reserve stock for ALL items
            foreach (var item in salesOrder.Items)
            {
                var stock = await _stockRepository.Query()
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == salesOrder.WarehouseId, ct);

                if (stock != null)
                {
                    stock.Reserve(item.Quantity);
                }
            }

            // Approve the sales order
            salesOrder.Approve(currentUserId);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            // Reload with navigation properties
            var updatedSo = await _salesOrderRepository.Query()
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .Include(s => s.Warehouse)
                .FirstOrDefaultAsync(s => s.Id == salesOrder.Id, ct);

            var dto = MapToDto(updatedSo!);

            return Result<SalesOrderDto>.SuccessResult(dto, "Sales order approved and stock reserved");
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
