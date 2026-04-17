using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.SalesOrders.Commands.CreateSalesOrder;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Commands.SubmitSalesOrder;

public class SubmitSalesOrderCommandHandler
    : IRequestHandler<SubmitSalesOrderCommand, SalesOrderDto>
{
    private readonly IUnitOfWork   _uow;
    private readonly IEmailService _emailService;

    public SubmitSalesOrderCommandHandler(
        IUnitOfWork uow, IEmailService emailService)
    {
        _uow          = uow;
        _emailService = emailService;
    }

    public async Task<SalesOrderDto> Handle(
        SubmitSalesOrderCommand request,
        CancellationToken cancellationToken)
    {
        var so = await _uow.SalesOrders.Query()
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (so == null)
            throw new NotFoundException("Sales order", request.Id);

        so.Submit();
        await _uow.SaveChangesAsync(cancellationToken);

        // Notify sales managers (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var managers = await _uow.Users.Query()
                    .Where(u => u.UserRoles.Any(ur =>
                        ur.Role.Name == "SalesManager" ||
                        ur.Role.Name == "Admin" ||
                        ur.Role.Name == "SuperAdmin"))
                    .Select(u => u.Email)
                    .ToListAsync(CancellationToken.None);

                foreach (var email in managers)
                    await _emailService.SendSalesOrderStatusAsync(
                        email, so.SoNumber, "Submitted", so.CustomerName);
            }
            catch { /* silent */ }
        });

        return BuildDto(so);
    }

    private static SalesOrderDto BuildDto(Domain.Entities.SalesOrder so)
        => CreateSalesOrderCommandHandler.MapToDto(
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
