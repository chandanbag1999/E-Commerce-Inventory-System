using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Stocks.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Stocks.Commands.AdjustStock;

public record AdjustStockCommand : IRequest<Result<AdjustStockResponseDto>>
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string AdjustmentType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID is required");

        RuleFor(x => x.AdjustmentType)
            .NotEmpty().WithMessage("Adjustment type is required")
            .Must(x => x == "Add" || x == "Remove").WithMessage("Adjustment type must be 'Add' or 'Remove'");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
    }
}

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, Result<AdjustStockResponseDto>>
{
    private readonly IRepository<Stock> _stockRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IRepository<StockMovement> _stockMovementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AdjustStockCommandHandler(
        IRepository<Stock> stockRepository,
        IRepository<Product> productRepository,
        IRepository<Warehouse> warehouseRepository,
        IRepository<StockMovement> stockMovementRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _stockRepository = stockRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _stockMovementRepository = stockMovementRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AdjustStockResponseDto>> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Validate product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId, ct);
            if (product == null)
                return Result<AdjustStockResponseDto>.FailureResult("Product not found");

            // Validate warehouse exists
            var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId, ct);
            if (warehouse == null)
                return Result<AdjustStockResponseDto>.FailureResult("Warehouse not found");

            // Find or create stock entry
            var stock = await _stockRepository.Query()
                .FirstOrDefaultAsync(s => s.ProductId == request.ProductId 
                                       && s.WarehouseId == request.WarehouseId, ct);

            if (stock == null)
            {
                // Auto-create stock entry at 0
                stock = Stock.Create(request.ProductId, request.WarehouseId, initialQty: 0);
                await _stockRepository.AddAsync(stock, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            // Perform adjustment using domain method
            StockMovement movement;
            if (request.AdjustmentType == "Add")
            {
                movement = stock.AddStock(
                    request.Quantity,
                    "ManualAdjustmentAdd",
                    null,
                    "Manual",
                    request.Reason,
                    _currentUserService.UserId ?? Guid.Empty);
            }
            else
            {
                movement = stock.RemoveStock(
                    request.Quantity,
                    "ManualAdjustmentRemove",
                    null,
                    "Manual",
                    request.Reason,
                    _currentUserService.UserId ?? Guid.Empty);
            }

            await _stockMovementRepository.AddAsync(movement, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            var response = new AdjustStockResponseDto(
                stock.ProductId,
                stock.WarehouseId,
                stock.Quantity,
                stock.AvailableQty,
                stock.ReservedQty,
                movement.MovementType,
                stock.UpdatedAt
            );

            return Result<AdjustStockResponseDto>.SuccessResult(response, "Stock adjusted successfully");
        }
        catch (BusinessRuleViolationException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            return Result<AdjustStockResponseDto>.FailureResult(ex.Message);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
