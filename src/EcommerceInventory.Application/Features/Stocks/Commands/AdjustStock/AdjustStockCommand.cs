using EcommerceInventory.Application.Features.Stocks.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Stocks.Commands.AdjustStock;

public class AdjustStockCommand : IRequest<StockAdjustmentResultDto>
{
    public Guid   ProductId      { get; set; }
    public Guid   WarehouseId    { get; set; }
    public string AdjustmentType { get; set; } = string.Empty;
    public int    Quantity       { get; set; }
    public string Reason         { get; set; } = string.Empty;
}
