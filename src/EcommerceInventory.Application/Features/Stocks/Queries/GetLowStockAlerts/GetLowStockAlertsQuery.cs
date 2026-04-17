using EcommerceInventory.Application.Features.Stocks.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Stocks.Queries.GetLowStockAlerts;

public class GetLowStockAlertsQuery : IRequest<List<LowStockAlertDto>>
{
    public Guid? WarehouseId { get; set; }
}
