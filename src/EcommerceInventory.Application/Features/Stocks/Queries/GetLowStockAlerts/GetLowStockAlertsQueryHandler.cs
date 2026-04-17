using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Stocks.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Stocks.Queries.GetLowStockAlerts;

public class GetLowStockAlertsQueryHandler
    : IRequestHandler<GetLowStockAlertsQuery, List<LowStockAlertDto>>
{
    private readonly IUnitOfWork _uow;

    public GetLowStockAlertsQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<List<LowStockAlertDto>> Handle(
        GetLowStockAlertsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _uow.Stocks.Query()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.Product.ReorderLevel > 0 &&
                        (s.Quantity - s.ReservedQty) <= s.Product.ReorderLevel)
            .AsQueryable();

        if (request.WarehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == request.WarehouseId.Value);

        var lowStockItems = await query
            .OrderBy(s => s.Quantity - s.ReservedQty)
            .ToListAsync(cancellationToken);

        return lowStockItems.Select(s => new LowStockAlertDto
        {
            ProductId    = s.ProductId,
            ProductName  = s.Product.Name,
            ProductSku   = s.Product.Sku,
            WarehouseId  = s.WarehouseId,
            WarehouseName = s.Warehouse.Name,
            CurrentQty   = s.Quantity,
            AvailableQty = s.AvailableQty,
            ReservedQty  = s.ReservedQty,
            ReorderLevel = s.Product.ReorderLevel,
            ReorderQty   = s.Product.ReorderQty,
            Deficit      = s.Product.ReorderLevel - s.AvailableQty
        }).ToList();
    }
}
