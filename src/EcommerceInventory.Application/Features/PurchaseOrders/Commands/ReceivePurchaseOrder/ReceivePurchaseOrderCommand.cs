using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;

public record ReceivePurchaseOrderCommand : IRequest<Result<PurchaseOrderDto>>
{
    public Guid PurchaseOrderId { get; set; }
    public List<ReceivePurchaseOrderItemDto> Items { get; set; } = new();
}

public class ReceivePurchaseOrderCommandValidator : AbstractValidator<ReceivePurchaseOrderCommand>
{
    public ReceivePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase order ID is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item to receive is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ItemId)
                .NotEmpty().WithMessage("Item ID is required");

            item.RuleFor(x => x.QuantityReceived)
                .GreaterThan(0).WithMessage("Quantity received must be greater than 0");
        });
    }
}

public class ReceivePurchaseOrderCommandHandler : IRequestHandler<ReceivePurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    private readonly IRepository<PurchaseOrder> _poRepository;
    private readonly IRepository<Stock> _stockRepository;
    private readonly IRepository<StockMovement> _stockMovementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReceivePurchaseOrderCommandHandler(
        IRepository<PurchaseOrder> poRepository,
        IRepository<Stock> stockRepository,
        IRepository<StockMovement> stockMovementRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _poRepository = poRepository;
        _stockRepository = stockRepository;
        _stockMovementRepository = stockMovementRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(ReceivePurchaseOrderCommand request, CancellationToken ct)
    {
        // Load PO with items
        var po = await _poRepository.Query()
            .Include(p => p.Items)
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .ThenInclude(w => w.Stocks)
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, ct);

        if (po == null)
            return Result<PurchaseOrderDto>.FailureResult("Purchase order not found");

        if (po.Status != EcommerceInventory.Domain.Enums.OrderStatus.Approved)
            return Result<PurchaseOrderDto>.FailureResult("Only Approved orders can be received");

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var performedBy = _currentUserService.UserId ?? Guid.Empty;

            // Process each received item
            foreach (var receivedItem in request.Items)
            {
                var poItem = po.Items.FirstOrDefault(i => i.Id == receivedItem.ItemId);
                if (poItem == null)
                    return Result<PurchaseOrderDto>.FailureResult($"Item {receivedItem.ItemId} not in this PO");

                if (receivedItem.QuantityReceived > poItem.QuantityOrdered)
                    return Result<PurchaseOrderDto>.FailureResult("Cannot receive more than ordered");

                // Update quantity received
                poItem.GetType().GetProperty("QuantityReceived")!.SetValue(poItem, receivedItem.QuantityReceived);

                // Find or create stock for (product, warehouse)
                var stock = await FindOrCreateStockAsync(poItem.ProductId, po.WarehouseId, ct);

                // Add stock via domain method + create movement record
                var movement = stock.AddStock(
                    qty: receivedItem.QuantityReceived,
                    movementType: "PurchaseReceived",
                    referenceId: po.Id,
                    referenceType: "PurchaseOrder",
                    notes: $"Received via {po.PoNumber}",
                    performedBy: performedBy
                );

                await _stockMovementRepository.AddAsync(movement, ct);
            }

            // Mark PO as Received
            po.MarkReceived();

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            // Reload with all data
            var updatedPo = await _poRepository.Query()
                .Include(p => p.Items).ThenInclude(i => i.Product)
                .Include(p => p.Supplier)
                .Include(p => p.Warehouse)
                .FirstOrDefaultAsync(p => p.Id == po.Id, ct);

            var dto = MapToDto(updatedPo!);

            return Result<PurchaseOrderDto>.SuccessResult(dto, "Purchase order received and stock updated");
        }
        catch (BusinessRuleViolationException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            return Result<PurchaseOrderDto>.FailureResult(ex.Message);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<Stock> FindOrCreateStockAsync(Guid productId, Guid warehouseId, CancellationToken ct)
    {
        var stock = await _stockRepository.Query()
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId, ct);

        if (stock == null)
        {
            stock = Stock.Create(productId, warehouseId, initialQty: 0);
            await _stockRepository.AddAsync(stock, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return stock;
    }

    private static PurchaseOrderDto MapToDto(PurchaseOrder po)
    {
        return new PurchaseOrderDto(
            po.Id,
            po.PoNumber,
            po.SupplierId,
            po.Supplier?.Name ?? string.Empty,
            po.WarehouseId,
            po.Warehouse?.Name ?? string.Empty,
            po.Status.ToString(),
            po.TotalAmount,
            po.Notes,
            po.ExpectedDeliveryAt,
            po.ApprovedBy,
            po.ApprovedAt,
            po.ReceivedAt,
            po.Items.Select(i => new PurchaseOrderItemDto(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? string.Empty,
                i.Product?.Sku ?? string.Empty,
                i.QuantityOrdered,
                i.QuantityReceived,
                i.UnitCost,
                i.TotalCost
            )).ToList(),
            po.CreatedAt,
            po.UpdatedAt
        );
    }
}
