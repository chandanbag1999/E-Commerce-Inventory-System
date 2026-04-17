using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Stocks.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Stocks.Commands.AdjustStock;

public class AdjustStockCommandHandler
    : IRequestHandler<AdjustStockCommand, StockAdjustmentResultDto>
{
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService       _emailService;

    public AdjustStockCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IEmailService emailService)
    {
        _uow          = uow;
        _currentUser  = currentUser;
        _emailService = emailService;
    }

    public async Task<StockAdjustmentResultDto> Handle(
        AdjustStockCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate product exists
        var product = await _uow.Products.GetByIdAsync(
            request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException("Product", request.ProductId);

        // 2. Validate warehouse exists
        var warehouse = await _uow.Warehouses.GetByIdAsync(
            request.WarehouseId, cancellationToken);
        if (warehouse == null)
            throw new NotFoundException("Warehouse", request.WarehouseId);

        await _uow.BeginTransactionAsync(cancellationToken);
        try
        {
            // 3. Find or create stock entry
            var stock = await _uow.Stocks.Query()
                .FirstOrDefaultAsync(
                    s => s.ProductId   == request.ProductId &&
                         s.WarehouseId == request.WarehouseId,
                    cancellationToken);

            if (stock == null)
            {
                stock = Stock.Create(request.ProductId, request.WarehouseId, 0);
                await _uow.Stocks.AddAsync(stock, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
            }

            var quantityBefore = stock.Quantity;

            // 4. Perform adjustment
            var movementType = request.AdjustmentType == "Add"
                ? "ManualAdjustmentAdd"
                : "ManualAdjustmentRemove";

            var movement = request.AdjustmentType == "Add"
                ? stock.AddStock(
                    request.Quantity, movementType,
                    null, "Manual",
                    request.Reason, _currentUser.UserId)
                : stock.RemoveStock(
                    request.Quantity, movementType,
                    null, "Manual",
                    request.Reason, _currentUser.UserId);

            await _uow.StockMovements.AddAsync(movement, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            await _uow.CommitTransactionAsync(cancellationToken);

            // 5. Low stock check (fire and forget)
            if (stock.IsLowStock(product.ReorderLevel))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var managers = await _uow.Users.Query()
                            .Where(u => u.UserRoles.Any(ur =>
                                ur.Role.Name == "InventoryManager" ||
                                ur.Role.Name == "Admin" ||
                                ur.Role.Name == "SuperAdmin"))
                            .Select(u => u.Email)
                            .ToListAsync(CancellationToken.None);

                        foreach (var email in managers)
                        {
                            await _emailService.SendLowStockAlertAsync(
                                email,
                                product.Name,
                                stock.AvailableQty,
                                product.ReorderLevel);
                        }
                    }
                    catch { /* silent - dont break main flow */ }
                });
            }

            return new StockAdjustmentResultDto
            {
                StockId         = stock.Id,
                ProductId       = product.Id,
                ProductName     = product.Name,
                WarehouseId     = warehouse.Id,
                WarehouseName   = warehouse.Name,
                QuantityBefore  = quantityBefore,
                QuantityAfter   = stock.Quantity,
                AdjustedBy      = request.Quantity,
                AdjustmentType  = request.AdjustmentType,
                Reason          = request.Reason,
                AvailableQty    = stock.AvailableQty,
                ReservedQty     = stock.ReservedQty
            };
        }
        catch
        {
            await _uow.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
