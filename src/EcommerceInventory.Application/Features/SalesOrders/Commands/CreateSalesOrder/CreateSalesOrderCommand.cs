using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;

public record CreateSalesOrderCommand : IRequest<Result<SalesOrderDto>>
{
    public Guid WarehouseId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }
    public string? ShippingAddressJson { get; set; }
    public List<CreateSalesOrderItemDto> Items { get; set; } = new();
}

public class CreateSalesOrderCommandValidator : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderCommandValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required")
            .Must(x => x.Count > 0).WithMessage("At least one item is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product is required for each item");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be >= 0");

            item.RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount must be >= 0");
        });
    }
}

public class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, Result<SalesOrderDto>>
{
    private readonly IRepository<SalesOrder> _salesOrderRepository;
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateSalesOrderCommandHandler(
        IRepository<SalesOrder> salesOrderRepository,
        IRepository<Warehouse> warehouseRepository,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _salesOrderRepository = salesOrderRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SalesOrderDto>> Handle(CreateSalesOrderCommand request, CancellationToken ct)
    {
        // Validate warehouse exists
        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId, ct);
        if (warehouse == null)
            return Result<SalesOrderDto>.FailureResult("Warehouse not found");

        // Validate all product IDs exist (batch query)
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var validProducts = await _productRepository.Query()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(ct);

        if (validProducts.Count != productIds.Count)
        {
            var missing = productIds.Except(validProducts);
            return Result<SalesOrderDto>.FailureResult($"Products not found: {string.Join(", ", missing)}");
        }

        // Generate SO number: SO-{yyyyMM}-{sequence:D5}
        var lastSo = await _salesOrderRepository.Query()
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => s.SoNumber)
            .FirstOrDefaultAsync(ct);

        var sequence = ExtractSequence(lastSo) + 1;
        var soNumber = $"SO-{DateTime.UtcNow:yyyyMM}-{sequence:D5}";

        // Create SalesOrder entity via factory
        var createdBy = _currentUserService.UserId ?? Guid.Empty;
        var salesOrder = SalesOrder.Create(
            soNumber,
            request.WarehouseId,
            createdBy,
            request.CustomerName,
            request.CustomerEmail,
            request.CustomerPhone,
            request.Notes,
            request.ShippingAddressJson);

        // Add items via domain method
        foreach (var item in request.Items)
        {
            salesOrder.AddItem(item.ProductId, item.Quantity, item.UnitPrice, item.Discount);
        }

        await _salesOrderRepository.AddAsync(salesOrder, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Reload with navigation properties
        var createdSo = await _salesOrderRepository.Query()
            .Include(s => s.Warehouse)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == salesOrder.Id, ct);

        var dto = MapToDto(createdSo!);

        return Result<SalesOrderDto>.SuccessResult(dto, "Sales order created successfully");
    }

    private static int ExtractSequence(string? lastSoNumber)
    {
        if (string.IsNullOrEmpty(lastSoNumber))
            return 0;

        // Extract the sequence number from format "SO-202504-00001"
        var parts = lastSoNumber.Split('-');
        if (parts.Length == 3 && int.TryParse(parts[2], out var seq))
            return seq;

        return 0;
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
