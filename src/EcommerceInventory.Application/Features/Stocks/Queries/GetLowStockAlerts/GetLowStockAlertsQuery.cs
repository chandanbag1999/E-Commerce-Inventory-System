using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Stocks.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Stocks.Queries.GetLowStockAlerts;

public record GetLowStockAlertsQuery : IRequest<Result<List<LowStockAlertDto>>>;

public class GetLowStockAlertsQueryHandler : IRequestHandler<GetLowStockAlertsQuery, Result<List<LowStockAlertDto>>>
{
    private readonly IRepository<Stock> _stockRepository;

    public GetLowStockAlertsQueryHandler(IRepository<Stock> stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<Result<List<LowStockAlertDto>>> Handle(GetLowStockAlertsQuery request, CancellationToken ct)
    {
        var lowStockItems = await _stockRepository.Query()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.Product.ReorderLevel > 0 && (s.Quantity - s.ReservedQty) <= s.Product.ReorderLevel)
            .OrderBy(s => (s.Quantity - s.ReservedQty))
            .ToListAsync(ct);

        var alerts = lowStockItems.Select(s => new LowStockAlertDto(
            s.ProductId,
            s.Product.Name,
            s.Product.Sku,
            s.WarehouseId,
            s.Warehouse.Name,
            s.Quantity,
            s.AvailableQty,
            s.Product.ReorderLevel,
            s.Product.ReorderQty,
            s.Product.ReorderLevel - s.AvailableQty
        )).ToList();

        return Result<List<LowStockAlertDto>>.SuccessResult(alerts);
    }
}
