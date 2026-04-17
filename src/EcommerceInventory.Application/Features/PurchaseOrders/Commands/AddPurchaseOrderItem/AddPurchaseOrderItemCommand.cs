using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.AddPurchaseOrderItem;

public class AddPurchaseOrderItemCommand : IRequest<PurchaseOrderDto>
{
    public Guid    PurchaseOrderId { get; set; }
    public Guid    ProductId       { get; set; }
    public int     QuantityOrdered { get; set; }
    public decimal UnitCost        { get; set; }
}
