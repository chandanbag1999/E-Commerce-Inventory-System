using EcommerceInventory.Application.Features.Stocks.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Stocks.Queries.GetStockByProduct;

public class GetStockByProductQuery : IRequest<List<StockDto>>
{
    public Guid ProductId { get; set; }
}
