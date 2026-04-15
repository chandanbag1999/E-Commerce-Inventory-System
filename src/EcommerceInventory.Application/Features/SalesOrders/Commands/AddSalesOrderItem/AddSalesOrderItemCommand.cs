using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.AddSalesOrderItem;

public record AddSalesOrderItemCommand : IRequest<Result<SalesOrderDto>>
{
    public Guid SalesOrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
}

public class AddSalesOrderItemCommandValidator : AbstractValidator<AddSalesOrderItemCommand>
{
    public AddSalesOrderItemCommandValidator()
    {
        RuleFor(x => x.SalesOrderId)
            .NotEmpty().WithMessage("Sales order ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price must be >= 0");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount must be >= 0");
    }
}

public class AddSalesOrderItemCommandHandler : IRequestHandler<AddSalesOrderItemCommand, Result<SalesOrderDto>>
{
    private readonly IRepository<SalesOrder> _salesOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddSalesOrderItemCommandHandler(
        IRepository<SalesOrder> salesOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _salesOrderRepository = salesOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SalesOrderDto>> Handle(AddSalesOrderItemCommand request, CancellationToken ct)
    {
        var salesOrder = await _salesOrderRepository.Query()
            .Include(s => s.Items)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.SalesOrderId, ct);

        if (salesOrder == null)
            return Result<SalesOrderDto>.FailureResult("Sales order not found");

        // Add item via domain method (validates Draft status)
        salesOrder.AddItem(request.ProductId, request.Quantity, request.UnitPrice, request.Discount);

        await _unitOfWork.SaveChangesAsync(ct);

        var dto = MapToDto(salesOrder);

        return Result<SalesOrderDto>.SuccessResult(dto, "Item added to sales order");
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
