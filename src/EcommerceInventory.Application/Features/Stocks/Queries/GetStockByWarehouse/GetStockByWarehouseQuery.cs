using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Stocks.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Stocks.Queries.GetStockByWarehouse;

public record GetStockByWarehouseQuery(Guid WarehouseId) : IRequest<Result<List<StockDto>>>;

public class GetStockByWarehouseQueryHandler : IRequestHandler<GetStockByWarehouseQuery, Result<List<StockDto>>>
{
    private readonly IRepository<Stock> _stockRepository;

    public GetStockByWarehouseQueryHandler(IRepository<Stock> stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<Result<List<StockDto>>> Handle(GetStockByWarehouseQuery request, CancellationToken ct)
    {
        var stocks = await _stockRepository.Query()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.WarehouseId == request.WarehouseId)
            .OrderBy(s => s.Product.Name)
            .ToListAsync(ct);

        var dtos = stocks.Select(MapToDto).ToList();

        return Result<List<StockDto>>.SuccessResult(dtos);
    }

    private static StockDto MapToDto(Stock stock)
    {
        return new StockDto(
            stock.Id,
            stock.ProductId,
            stock.Product.Name,
            stock.Product.Sku,
            stock.WarehouseId,
            stock.Warehouse.Name,
            stock.Warehouse.Code,
            stock.Quantity,
            stock.ReservedQty,
            stock.AvailableQty,
            stock.LastCountedAt,
            stock.UpdatedAt
        );
    }
}
