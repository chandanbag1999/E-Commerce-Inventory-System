using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.RejectPurchaseOrder;

public class RejectPurchaseOrderCommandHandler
    : IRequestHandler<RejectPurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IUnitOfWork   _uow;
    private readonly IEmailService _emailService;

    public RejectPurchaseOrderCommandHandler(
        IUnitOfWork uow, IEmailService emailService)
    {
        _uow          = uow;
        _emailService = emailService;
    }

    public async Task<PurchaseOrderDto> Handle(
        RejectPurchaseOrderCommand request,
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

        po.Reject();
        await _uow.SaveChangesAsync(cancellationToken);

        // Notify creator (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var creator = await _uow.Users.GetByIdAsync(
                    po.CreatedBy, CancellationToken.None);
                if (creator != null)
                    await _emailService.SendPurchaseOrderStatusAsync(
                        creator.Email, po.PoNumber, "Rejected");
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
