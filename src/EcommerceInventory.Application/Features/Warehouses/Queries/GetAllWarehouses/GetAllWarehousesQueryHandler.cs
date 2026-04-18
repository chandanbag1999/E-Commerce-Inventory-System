using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Warehouses.Queries.GetAllWarehouses;

public class GetAllWarehousesQueryHandler
    : IRequestHandler<GetAllWarehousesQuery, PagedResult<WarehouseListDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllWarehousesQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<PagedResult<WarehouseListDto>> Handle(
        GetAllWarehousesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _uow.Warehouses.Query().AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(w => w.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower().Trim();
            query = query.Where(w =>
                w.Name.ToLower().Contains(search) ||
                w.Code.ToLower().Contains(search) ||
                (w.Address != null && w.Address.City.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(w => w.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WarehouseListDto
            {
                Id          = w.Id,
                Name        = w.Name,
                Code        = w.Code,
                IsActive    = w.IsActive,
                Status      = w.IsActive ? "Active" : "Inactive",
                Phone       = w.Phone,
                Email       = w.Email,
                Capacity    = w.Capacity,
                ManagerName = w.Manager != null ? w.Manager.FullName : null,
                TotalStockLines = w.Stocks.Count,
                Version     = w.Version,
                AddressString   = w.Address != null
                    ? w.Address.Street + ", " + w.Address.City + ", "
                      + w.Address.State + " - " + w.Address.Pincode + ", "
                      + w.Address.Country
                    : null,
                CreatedAt = w.CreatedAt
            })
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.Utilization = item.Capacity.HasValue && item.Capacity.Value > 0
                ? Math.Round((double)item.TotalStockLines / item.Capacity.Value * 100, 1)
                : null;
        }

        return new PagedResult<WarehouseListDto>
        {
            Items      = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize   = request.PageSize
        };
    }
}
