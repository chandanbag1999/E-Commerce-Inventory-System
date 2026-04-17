using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;

public class ApprovePurchaseOrderCommandHandler
    : IRequestHandler<ApprovePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService       _emailService;

    public ApprovePurchaseOrderCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IEmailService emailService)
    {
        _uow         = uow;
        _currentUser = currentUser;
        _emailService = emailService;
    }

    public async Task<PurchaseOrderDto> Handle(
        ApprovePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        var po = await _uow.PurchaseOrders.Query()
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (po == null)
            throw new NotFoundException("Purchase order", request.Id);

        // 4-eye rule: approver cannot be same as creator
        if (po.CreatedBy == _currentUser.UserId)
            throw new BusinessRuleViolationException(
                "You cannot approve a purchase order you created (4-eye principle).");

        po.Approve(_currentUser.UserId!.Value);
        await _uow.SaveChangesAsync(cancellationToken);

        // Notify (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var creator = await _uow.Users.GetByIdAsync(
                    po.CreatedBy, CancellationToken.None);
                if (creator != null)
                    await _emailService.SendPurchaseOrderStatusAsync(
                        creator.Email, po.PoNumber, "Approved");
            }
            catch { /* silent */ }
        });

        var approver = await _uow.Users.GetByIdAsync(
            _currentUser.UserId!.Value, cancellationToken);

        return CreatePurchaseOrderCommandHandler.MapToDto(
            po,
            po.Supplier.Name,
            po.Warehouse.Name,
            null,
            approver?.FullName,
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
