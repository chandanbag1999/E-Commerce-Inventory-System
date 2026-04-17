using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;

public class ApprovePurchaseOrderCommand : IRequest<PurchaseOrderDto>
{
    public Guid Id { get; set; }
}
