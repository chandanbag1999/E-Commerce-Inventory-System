using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.ShipSalesOrder;

public record ShipSalesOrderCommand(Guid SalesOrderId) : IRequest<Result<SalesOrderDto>>;

public class ShipSalesOrderCommandHandler : IRequestHandler<ShipSalesOrderCommand, Result<SalesOrderDto>>
{
    private readonly IRepository<SalesOrder> _salesOrderRepository;
    private readonly IRepository<Stock> _stockRepository;
    private readonly IRepository<StockMovement> _stockMovementRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public ShipSalesOrderCommandHandler(
        IRepository<SalesOrder> salesOrderRepository,
        IRepository<Stock> stockRepository,
        IRepository<StockMovement> stockMovementRepository,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _salesOrderRepository = salesOrderRepository;
        _stockRepository = stockRepository;
        _stockMovementRepository = stockMovementRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<Result<SalesOrderDto>> Handle(ShipSalesOrderCommand request, CancellationToken ct)
    {
        // Load SO with items
        var salesOrder = await _salesOrderRepository.Query()
            .Include(s => s.Items)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.SalesOrderId, ct);

        if (salesOrder == null)
            return Result<SalesOrderDto>.FailureResult("Sales order not found");

        if (salesOrder.Status != OrderStatus.Approved)
            return Result<SalesOrderDto>.FailureResult("Only Approved orders can be shipped");

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var currentUserId = _currentUserService.UserId ?? Guid.Empty;

            // For each item: release reservation + deduct stock + create movement
            foreach (var item in salesOrder.Items)
            {
                var stock = await _stockRepository.Query()
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == salesOrder.WarehouseId, ct);

                if (stock == null)
                    return Result<SalesOrderDto>.FailureResult($"Stock not found for product {item.ProductId}");

                // Step A: Release reservation
                stock.ReleaseReservation(item.Quantity);

                // Step B: Deduct actual stock and create movement record
                var movement = stock.RemoveStock(
                    qty: item.Quantity,
                    movementType: "SaleDispatched",
                    referenceId: salesOrder.Id,
                    referenceType: "SalesOrder",
                    notes: $"Shipped via {salesOrder.SoNumber}",
                    performedBy: currentUserId
                );

                await _stockMovementRepository.AddAsync(movement, ct);

                // Step C: Low stock check + email alert
                var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
                if (product != null && stock.IsLowStock(product.ReorderLevel))
                {
                    // Fire-and-forget email (non-blocking)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendLowStockAlertAsync(
                                productName: product.Name,
                                currentQty: stock.AvailableQty,
                                reorderLevel: product.ReorderLevel
                            );
                        }
                        catch
                        {
                            // Log but don't fail the shipment
                        }
                    }, ct);
                }
            }

            // Mark as shipped
            salesOrder.Ship();

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            // Reload with navigation properties
            var updatedSo = await _salesOrderRepository.Query()
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .Include(s => s.Warehouse)
                .FirstOrDefaultAsync(s => s.Id == salesOrder.Id, ct);

            var dto = MapToDto(updatedSo!);

            return Result<SalesOrderDto>.SuccessResult(dto, "Sales order shipped and stock deducted");
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
