using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.RejectPurchaseOrder;

public class RejectPurchaseOrderCommand : IRequest<PurchaseOrderDto>
{
    public Guid    Id     { get; set; }
    public string? Reason { get; set; }
}
