using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommand : IRequest<PurchaseOrderDto>
{
    public Guid      SupplierId          { get; set; }
    public Guid      WarehouseId         { get; set; }
    public DateTime? ExpectedDeliveryAt  { get; set; }
    public string?   Notes               { get; set; }
    public List<AddPurchaseOrderItemRequest> Items { get; set; } = new();
}
