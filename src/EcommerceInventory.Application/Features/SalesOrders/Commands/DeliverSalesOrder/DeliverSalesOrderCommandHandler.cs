using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.DeliverSalesOrder;

public class DeliverSalesOrderCommandHandler
    : IRequestHandler<DeliverSalesOrderCommand, SalesOrderDto>
{
    private readonly IUnitOfWork   _uow;
    private readonly IEmailService _emailService;

    public DeliverSalesOrderCommandHandler(
        IUnitOfWork uow, IEmailService emailService)
    {
        _uow          = uow;
        _emailService = emailService;
    }

    public async Task<SalesOrderDto> Handle(
        DeliverSalesOrderCommand request,
        CancellationToken cancellationToken)
    {
        var so = await _uow.SalesOrders.Query()
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (so == null)
            throw new NotFoundException("Sales order", request.Id);

        so.Deliver();
        await _uow.SaveChangesAsync(cancellationToken);

        // Notify (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var creator = await _uow.Users.GetByIdAsync(
                    so.CreatedBy, CancellationToken.None);
                if (creator != null)
                    await _emailService.SendSalesOrderStatusAsync(
                        creator.Email, so.SoNumber, "Delivered", so.CustomerName);
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
