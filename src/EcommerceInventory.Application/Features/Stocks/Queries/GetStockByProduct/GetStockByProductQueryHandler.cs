using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Stocks.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Stocks.Queries.GetStockByProduct;

public class GetStockByProductQueryHandler
    : IRequestHandler<GetStockByProductQuery, List<StockDto>>
{
    private readonly IUnitOfWork _uow;

    public GetStockByProductQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<List<StockDto>> Handle(
        GetStockByProductQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _uow.Products.GetByIdAsync(
            request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException("Product", request.ProductId);

        var stocks = await _uow.Stocks.Query()
            .Include(s => s.Warehouse)
            .Include(s => s.Product)
                .ThenInclude(p => p.Images)
            .Where(s => s.ProductId == request.ProductId)
            .OrderBy(s => s.Warehouse.Name)
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
