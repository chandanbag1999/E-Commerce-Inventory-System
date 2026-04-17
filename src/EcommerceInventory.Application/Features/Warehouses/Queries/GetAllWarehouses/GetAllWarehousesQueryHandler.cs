using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Warehouses.Queries.GetAllWarehouses;

public class GetAllWarehousesQueryHandler
    : IRequestHandler<GetAllWarehousesQuery, List<WarehouseListDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllWarehousesQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<List<WarehouseListDto>> Handle(
        GetAllWarehousesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _uow.Warehouses.Query()
            .Include(w => w.Manager)
            .Include(w => w.Stocks)
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(w => w.IsActive == request.IsActive.Value);

        var warehouses = await query
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        return warehouses.Select(w => new WarehouseListDto
        {
            Id          = w.Id,
            Name        = w.Name,
            Code        = w.Code,
            IsActive    = w.IsActive,
            Phone       = w.Phone,
            ManagerName = w.Manager?.FullName,
            TotalStockLines = w.Stocks.Count,
            CreatedAt   = w.CreatedAt
        }).ToList();
    }
}
