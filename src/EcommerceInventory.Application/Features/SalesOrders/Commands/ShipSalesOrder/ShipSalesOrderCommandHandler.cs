using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.ShipSalesOrder;

public class ShipSalesOrderCommandHandler
    : IRequestHandler<ShipSalesOrderCommand, SalesOrderDto>
{
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService       _emailService;

    public ShipSalesOrderCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IEmailService emailService)
    {
        _uow          = uow;
        _currentUser  = currentUser;
        _emailService = emailService;
    }

    public async Task<SalesOrderDto> Handle(
        ShipSalesOrderCommand request,
        CancellationToken cancellationToken)
    {
        var so = await _uow.SalesOrders.Query()
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (so == null)
            throw new NotFoundException("Sales order", request.Id);

        if (so.Status != OrderStatus.Approved)
            throw new BusinessRuleViolationException(
                "Only Approved orders can be shipped.");

        await _uow.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var item in so.Items)
            {
                var stock = await _uow.Stocks.Query()
                    .FirstOrDefaultAsync(
                        s => s.ProductId   == item.ProductId &&
                             s.WarehouseId == so.WarehouseId,
                        cancellationToken);

                if (stock == null)
                    throw new BusinessRuleViolationException(
                        $"Stock record not found for '{item.Product.Name}'.");

                // Step A: Release reservation
                stock.ReleaseReservation(item.Quantity);

                // Step B: Deduct actual stock
                var movement = stock.RemoveStock(
                    qty:           item.Quantity,
                    movementType:  "SaleDispatched",
                    referenceId:   so.Id,
                    referenceType: "SalesOrder",
                    notes:         $"Shipped via {so.SoNumber}",
                    performedBy:   _currentUser.UserId);

                await _uow.StockMovements.AddAsync(movement, cancellationToken);

                // Step C: Low stock check (fire and forget)
                var product = item.Product;
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
                                await _emailService.SendLowStockAlertAsync(
                                    email, product.Name,
                                    stock.AvailableQty, product.ReorderLevel);
                        }
                        catch { /* silent */ }
                    });
                }
            }

            so.Ship();
            await _uow.SaveChangesAsync(cancellationToken);
            await _uow.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        // Notify (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var creator = await _uow.Users.GetByIdAsync(
                    so.CreatedBy, CancellationToken.None);
                if (creator != null)
                    await _emailService.SendSalesOrderStatusAsync(
                        creator.Email, so.SoNumber, "Shipped", so.CustomerName);
            }
            catch { /* silent */ }
        });

        return CreateSalesOrderCommandHandler.MapToDto(
            so, so.Warehouse.Name, null, null,
            so.Items.Select(i => new SalesOrderItemDto
            {
                Id          = i.Id,
                ProductId   = i.ProductId,
                ProductName = i.Product.Name,
                ProductSku  = i.Product.Sku,
                Quantity    = i.Quantity,
                UnitPrice   = i.UnitPrice,
                Discount    = i.Discount,
                LineTotal   = i.LineTotal
            }).ToList());
    }
}
