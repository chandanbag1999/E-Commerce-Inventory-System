using EcommerceInventory.Application.Features.Stocks.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Stocks.Queries.GetStockByWarehouse;

public class GetStockByWarehouseQuery : IRequest<List<StockDto>>
{
    public Guid WarehouseId { get; set; }
}
