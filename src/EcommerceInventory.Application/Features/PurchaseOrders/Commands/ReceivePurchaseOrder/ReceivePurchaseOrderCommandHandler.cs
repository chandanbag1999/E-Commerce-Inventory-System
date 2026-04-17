using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;

public class ReceivePurchaseOrderCommandHandler
    : IRequestHandler<ReceivePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService       _emailService;

    public ReceivePurchaseOrderCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IEmailService emailService)
    {
        _uow          = uow;
        _currentUser  = currentUser;
        _emailService = emailService;
    }

    public async Task<PurchaseOrderDto> Handle(
        ReceivePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load PO with all includes
        var po = await _uow.PurchaseOrders.Query()
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (po == null)
            throw new NotFoundException("Purchase order", request.Id);

        if (po.Status != OrderStatus.Approved)
            throw new BusinessRuleViolationException(
                "Only Approved purchase orders can be received.");

        // 2. Validate all item IDs exist in PO
        foreach (var receivedItem in request.Items)
        {
            var poItem = po.Items.FirstOrDefault(
                i => i.Id == receivedItem.ItemId);
            if (poItem == null)
                throw new NotFoundException(
                    $"Item {receivedItem.ItemId} not found in this purchase order.");

            if (receivedItem.QuantityReceived > poItem.QuantityOrdered)
                throw new BusinessRuleViolationException(
                    $"Cannot receive {receivedItem.QuantityReceived} units for " +
                    $"'{poItem.Product.Name}'. Ordered: {poItem.QuantityOrdered}.");
        }

        await _uow.BeginTransactionAsync(cancellationToken);
        try
        {
            // 3. Process each item — update stock
            foreach (var receivedItem in request.Items)
            {
                var poItem = po.Items.First(i => i.Id == receivedItem.ItemId);
                poItem.QuantityReceived = receivedItem.QuantityReceived;

                // Find or create stock entry
                var stock = await _uow.Stocks.Query()
                    .FirstOrDefaultAsync(
                        s => s.ProductId   == poItem.ProductId &&
                             s.WarehouseId == po.WarehouseId,
                        cancellationToken);

                if (stock == null)
                {
                    stock = Stock.Create(poItem.ProductId, po.WarehouseId, 0);
                    await _uow.Stocks.AddAsync(stock, cancellationToken);
                    await _uow.SaveChangesAsync(cancellationToken);
                }

                // Add stock via domain method
                var movement = stock.AddStock(
                    qty:           receivedItem.QuantityReceived,
                    movementType:  "PurchaseReceived",
                    referenceId:   po.Id,
                    referenceType: "PurchaseOrder",
                    notes:         $"Received via {po.PoNumber}",
                    performedBy:   _currentUser.UserId);

                await _uow.StockMovements.AddAsync(movement, cancellationToken);

                // Low stock check (after adding, if it was low before)
                var product = poItem.Product;
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

            // 4. Mark PO as Received
            po.MarkReceived();
            await _uow.SaveChangesAsync(cancellationToken);
            await _uow.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        // 5. Notify (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var creator = await _uow.Users.GetByIdAsync(
                    po.CreatedBy, CancellationToken.None);
                if (creator != null)
                    await _emailService.SendPurchaseOrderStatusAsync(
                        creator.Email, po.PoNumber, "Received");
            }
            catch { /* silent */ }
        });

        return CreatePurchaseOrderCommandHandler.MapToDto(
            po, po.Supplier.Name, po.Warehouse.Name, null, null,
            po.Items.Select(i => new PurchaseOrderItemDto
            {
                Id               = i.Id,
                ProductId        = i.ProductId,
                ProductName      = i.Product.Name,
                ProductSku       = i.Product.Sku,
                QuantityOrdered  = i.QuantityOrdered,
                QuantityReceived = i.QuantityReceived,
                UnitCost         = i.UnitCost,
                TotalCost        = i.TotalCost
            }).ToList());
    }
}
