using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.SubmitPurchaseOrder;

public class SubmitPurchaseOrderCommandHandler
    : IRequestHandler<SubmitPurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IUnitOfWork   _uow;
    private readonly IEmailService _emailService;

    public SubmitPurchaseOrderCommandHandler(
        IUnitOfWork uow, IEmailService emailService)
    {
        _uow          = uow;
        _emailService = emailService;
    }

    public async Task<PurchaseOrderDto> Handle(
        SubmitPurchaseOrderCommand request,
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

        po.Submit();
        await _uow.SaveChangesAsync(cancellationToken);

        // Notify purchase managers (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var managers = await _uow.Users.Query()
                    .Where(u => u.UserRoles.Any(ur =>
                        ur.Role.Name == "PurchaseManager" ||
                        ur.Role.Name == "Admin" ||
                        ur.Role.Name == "SuperAdmin"))
                    .Select(u => u.Email)
                    .ToListAsync(CancellationToken.None);

                foreach (var email in managers)
                    await _emailService.SendPurchaseOrderStatusAsync(
                        email, po.PoNumber, "Submitted");
            }
            catch { /* silent */ }
        });

        return BuildDto(po);
    }

    private static PurchaseOrderDto BuildDto(Domain.Entities.PurchaseOrder po)
        => CreatePurchaseOrderCommandHandler.MapToDto(
            po,
            po.Supplier.Name,
            po.Warehouse.Name,
            null, null,
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
