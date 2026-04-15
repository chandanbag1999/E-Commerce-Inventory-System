using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.SalesOrders.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.SalesOrders.Queries.GetAllSalesOrders;

public record GetAllSalesOrdersQuery : IRequest<Result<List<SalesOrderListDto>>>;

public class GetAllSalesOrdersQueryHandler : IRequestHandler<GetAllSalesOrdersQuery, Result<List<SalesOrderListDto>>>
{
    private readonly IRepository<SalesOrder> _salesOrderRepository;

    public GetAllSalesOrdersQueryHandler(IRepository<SalesOrder> salesOrderRepository)
    {
        _salesOrderRepository = salesOrderRepository;
    }

    public async Task<Result<List<SalesOrderListDto>>> Handle(GetAllSalesOrdersQuery request, CancellationToken ct)
    {
        var salesOrders = await _salesOrderRepository.Query()
            .Include(s => s.Warehouse)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        var dtos = salesOrders.Select(so => new SalesOrderListDto(
            so.Id,
            so.SoNumber,
            so.CustomerName,
            so.Warehouse?.Name ?? string.Empty,
            so.Status.ToString(),
            so.TotalAmount,
            so.CreatedAt
        )).ToList();

        return Result<List<SalesOrderListDto>>.SuccessResult(dtos);
    }
}
