using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.ApproveSalesOrder;

public class ApproveSalesOrderCommandHandler
    : IRequestHandler<ApproveSalesOrderCommand, SalesOrderDto>
{
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService       _emailService;

    public ApproveSalesOrderCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IEmailService emailService)
    {
        _uow          = uow;
        _currentUser  = currentUser;
        _emailService = emailService;
    }

    public async Task<SalesOrderDto> Handle(
        ApproveSalesOrderCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load SO with all items and products
        var so = await _uow.SalesOrders.Query()
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (so == null)
            throw new NotFoundException("Sales order", request.Id);

        if (so.Status != OrderStatus.Submitted)
            throw new BusinessRuleViolationException(
                "Only Submitted orders can be approved.");

        // 2. 4-eye rule
        if (so.CreatedBy == _currentUser.UserId)
            throw new BusinessRuleViolationException(
                "You cannot approve an order you created (4-eye principle).");

        // 3. PRE-CHECK all items stock BEFORE making any changes
        var stockChecks = new List<(Domain.Entities.Stock stock,
                                    Domain.Entities.SalesOrderItem item)>();

        foreach (var item in so.Items)
        {
            var stock = await _uow.Stocks.Query()
                .FirstOrDefaultAsync(
                    s => s.ProductId   == item.ProductId &&
                         s.WarehouseId == so.WarehouseId,
                    cancellationToken);

            if (stock == null || stock.AvailableQty < item.Quantity)
            {
                var available = stock?.AvailableQty ?? 0;
                throw new BusinessRuleViolationException(
                    $"Insufficient stock for '{item.Product.Name}'. " +
                    $"Available: {available}, Required: {item.Quantity}.");
            }

            stockChecks.Add((stock, item));
        }

        await _uow.BeginTransactionAsync(cancellationToken);
        try
        {
            // 4. Reserve stock for ALL items
            foreach (var (stock, item) in stockChecks)
                stock.Reserve(item.Quantity);

            // 5. Approve SO
            so.Approve(_currentUser.UserId!.Value);
            await _uow.SaveChangesAsync(cancellationToken);
            await _uow.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        // 6. Notify (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var creator = await _uow.Users.GetByIdAsync(
                    so.CreatedBy, CancellationToken.None);
                if (creator != null)
                    await _emailService.SendSalesOrderStatusAsync(
                        creator.Email, so.SoNumber, "Approved", so.CustomerName);
            }
            catch { /* silent */ }
        });

        var approver = await _uow.Users.GetByIdAsync(
            _currentUser.UserId!.Value, cancellationToken);

        return CreateSalesOrderCommandHandler.MapToDto(
            so, so.Warehouse.Name, null, approver?.FullName,
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
