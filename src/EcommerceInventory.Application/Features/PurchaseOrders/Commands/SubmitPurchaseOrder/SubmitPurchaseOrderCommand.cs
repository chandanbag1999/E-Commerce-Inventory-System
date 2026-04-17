using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.SubmitPurchaseOrder;

public class SubmitPurchaseOrderCommand : IRequest<PurchaseOrderDto>
{
    public Guid Id { get; set; }
}
