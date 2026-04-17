using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Stocks.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Stocks.Queries.GetStockByWarehouse;

public class GetStockByWarehouseQueryHandler
    : IRequestHandler<GetStockByWarehouseQuery, List<StockDto>>
{
    private readonly IUnitOfWork _uow;

    public GetStockByWarehouseQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<List<StockDto>> Handle(
        GetStockByWarehouseQuery request,
        CancellationToken cancellationToken)
    {
        var warehouse = await _uow.Warehouses.GetByIdAsync(
            request.WarehouseId, cancellationToken);
        if (warehouse == null)
            throw new NotFoundException("Warehouse", request.WarehouseId);

        var stocks = await _uow.Stocks.Query()
            .Include(s => s.Product)
                .ThenInclude(p => p.Images)
            .Include(s => s.Warehouse)
            .Where(s => s.WarehouseId == request.WarehouseId)
            .OrderBy(s => s.Product.Name)
            .ToListAsync(cancellationToken);

        return stocks.Select(s => new StockDto
        {
            Id           = s.Id,
            ProductId    = s.ProductId,
            ProductName  = s.Product.Name,
            ProductSku   = s.Product.Sku,
            PrimaryImage = s.Product.Images
                            .FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                         ?? s.Product.Images.FirstOrDefault()?.ImageUrl,
            WarehouseId   = s.WarehouseId,
            WarehouseName = s.Warehouse.Name,
            WarehouseCode = s.Warehouse.Code,
            Quantity      = s.Quantity,
            ReservedQty   = s.ReservedQty,
            AvailableQty  = s.AvailableQty,
            ReorderLevel  = s.Product.ReorderLevel,
            IsLowStock    = s.IsLowStock(s.Product.ReorderLevel),
            LastCountedAt = s.LastCountedAt,
            UpdatedAt     = s.UpdatedAt
        }).ToList();
    }
}
