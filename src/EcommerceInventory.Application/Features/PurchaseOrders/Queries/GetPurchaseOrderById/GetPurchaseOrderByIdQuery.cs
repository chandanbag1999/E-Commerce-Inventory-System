using EcommerceInventory.Application.Features.PurchaseOrders.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;

public class GetPurchaseOrderByIdQuery : IRequest<PurchaseOrderDto>
{
    public Guid Id { get; set; }
}
