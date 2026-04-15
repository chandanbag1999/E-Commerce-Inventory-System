using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.RemovePurchaseOrderItem;

public record RemovePurchaseOrderItemCommand(Guid PurchaseOrderId, Guid ItemId) : IRequest<Result<PurchaseOrderDto>>;

public class RemovePurchaseOrderItemCommandHandler : IRequestHandler<RemovePurchaseOrderItemCommand, Result<PurchaseOrderDto>>
{
    private readonly IRepository<PurchaseOrder> _poRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemovePurchaseOrderItemCommandHandler(
        IRepository<PurchaseOrder> poRepository,
        IUnitOfWork unitOfWork)
    {
        _poRepository = poRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(RemovePurchaseOrderItemCommand request, CancellationToken ct)
    {
        var po = await _poRepository.Query()
            .Include(p => p.Items)
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, ct);

        if (po == null)
            return Result<PurchaseOrderDto>.FailureResult("Purchase order not found");

        // Remove item via domain method (validates Draft status)
        po.RemoveItem(request.ItemId);

        await _unitOfWork.SaveChangesAsync(ct);

        var dto = MapToDto(po);

        return Result<PurchaseOrderDto>.SuccessResult(dto, "Item removed from purchase order");
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
