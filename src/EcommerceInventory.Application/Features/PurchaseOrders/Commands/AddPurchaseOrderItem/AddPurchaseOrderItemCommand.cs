using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.AddPurchaseOrderItem;

public record AddPurchaseOrderItemCommand : IRequest<Result<PurchaseOrderDto>>
{
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public int QuantityOrdered { get; set; }
    public decimal UnitCost { get; set; }
}

public class AddPurchaseOrderItemCommandValidator : AbstractValidator<AddPurchaseOrderItemCommand>
{
    public AddPurchaseOrderItemCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty().WithMessage("Purchase order ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required");

        RuleFor(x => x.QuantityOrdered)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0).WithMessage("Unit cost must be >= 0");
    }
}

public class AddPurchaseOrderItemCommandHandler : IRequestHandler<AddPurchaseOrderItemCommand, Result<PurchaseOrderDto>>
{
    private readonly IRepository<PurchaseOrder> _poRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddPurchaseOrderItemCommandHandler(
        IRepository<PurchaseOrder> poRepository,
        IUnitOfWork unitOfWork)
    {
        _poRepository = poRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(AddPurchaseOrderItemCommand request, CancellationToken ct)
    {
        var po = await _poRepository.Query()
            .Include(p => p.Items)
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, ct);

        if (po == null)
            return Result<PurchaseOrderDto>.FailureResult("Purchase order not found");

        // Add item via domain method (validates Draft status)
        po.AddItem(request.ProductId, request.QuantityOrdered, request.UnitCost);

        await _unitOfWork.SaveChangesAsync(ct);

        var dto = MapToDto(po);

        return Result<PurchaseOrderDto>.SuccessResult(dto, "Item added to purchase order");
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
