using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;

public class ReceivePurchaseOrderCommand : IRequest<PurchaseOrderDto>
{
    public Guid Id    { get; set; }
    public List<ReceivePurchaseOrderItemRequest> Items { get; set; } = new();
}
