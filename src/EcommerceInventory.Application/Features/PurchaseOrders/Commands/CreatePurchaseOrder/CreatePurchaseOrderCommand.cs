using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public record CreatePurchaseOrderCommand : IRequest<Result<PurchaseOrderDto>>
{
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime? ExpectedDeliveryAt { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
}

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier is required");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required")
            .Must(x => x.Count > 0).WithMessage("At least one item is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product is required for each item");

            item.RuleFor(x => x.QuantityOrdered)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            item.RuleFor(x => x.UnitCost)
                .GreaterThanOrEqualTo(0).WithMessage("Unit cost must be >= 0");
        });
    }
}

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    private readonly IRepository<PurchaseOrder> _poRepository;
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreatePurchaseOrderCommandHandler(
        IRepository<PurchaseOrder> poRepository,
        IRepository<Supplier> supplierRepository,
        IRepository<Warehouse> warehouseRepository,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _poRepository = poRepository;
        _supplierRepository = supplierRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PurchaseOrderDto>> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
    {
        // Validate supplier exists
        var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId, ct);
        if (supplier == null)
            return Result<PurchaseOrderDto>.FailureResult("Supplier not found");

        // Validate warehouse exists
        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId, ct);
        if (warehouse == null)
            return Result<PurchaseOrderDto>.FailureResult("Warehouse not found");

        // Validate all product IDs exist (batch query)
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var validProducts = await _productRepository.Query()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(ct);

        if (validProducts.Count != productIds.Count)
        {
            var missing = productIds.Except(validProducts);
            return Result<PurchaseOrderDto>.FailureResult($"Products not found: {string.Join(", ", missing)}");
        }

        // Generate PO number
        var lastPo = await _poRepository.Query()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => p.PoNumber)
            .FirstOrDefaultAsync(ct);

        var sequence = ExtractSequence(lastPo) + 1;
        var poNumber = $"PO-{DateTime.UtcNow:yyyyMM}-{sequence:D5}";

        // Create PurchaseOrder entity
        var createdBy = _currentUserService.UserId ?? Guid.Empty;
        var po = PurchaseOrder.Create(poNumber, request.SupplierId, request.WarehouseId, createdBy, request.Notes);
        
        if (request.ExpectedDeliveryAt.HasValue)
        {
            // Need to set this via a property since factory doesn't have it
            po.GetType().GetProperty("ExpectedDeliveryAt")!.SetValue(po, request.ExpectedDeliveryAt.Value);
        }

        // Add items via domain method
        foreach (var item in request.Items)
        {
            po.AddItem(item.ProductId, item.QuantityOrdered, item.UnitCost);
        }

        await _poRepository.AddAsync(po, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Reload with navigation properties
        var createdPo = await _poRepository.Query()
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .Include(p => p.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(p => p.Id == po.Id, ct);

        var dto = MapToDto(createdPo!);

        return Result<PurchaseOrderDto>.SuccessResult(dto, "Purchase order created successfully");
    }

    private static int ExtractSequence(string? lastPoNumber)
    {
        if (string.IsNullOrEmpty(lastPoNumber))
            return 0;

        // Extract the sequence number from format "PO-202504-00001"
        var parts = lastPoNumber.Split('-');
        if (parts.Length == 3 && int.TryParse(parts[2], out var seq))
            return seq;

        return 0;
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
