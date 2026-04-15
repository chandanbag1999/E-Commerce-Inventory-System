using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Stocks.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Stocks.Queries.GetStockByProduct;

public record GetStockByProductQuery(Guid ProductId) : IRequest<Result<List<StockDto>>>;

public class GetStockByProductQueryHandler : IRequestHandler<GetStockByProductQuery, Result<List<StockDto>>>
{
    private readonly IRepository<Stock> _stockRepository;

    public GetStockByProductQueryHandler(IRepository<Stock> stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<Result<List<StockDto>>> Handle(GetStockByProductQuery request, CancellationToken ct)
    {
        var stocks = await _stockRepository.Query()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.ProductId == request.ProductId)
            .OrderBy(s => s.Warehouse.Name)
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
