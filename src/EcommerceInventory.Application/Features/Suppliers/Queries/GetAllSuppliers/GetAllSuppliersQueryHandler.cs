using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Suppliers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Suppliers.Queries.GetAllSuppliers;

public class GetAllSuppliersQueryHandler
    : IRequestHandler<GetAllSuppliersQuery, PagedResult<SupplierListDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllSuppliersQueryHandler(IUnitOfWork uow)
        => _uow = uow;

    public async Task<PagedResult<SupplierListDto>> Handle(
        GetAllSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _uow.Suppliers.Query()
            .Include(s => s.PurchaseOrders)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(term) ||
                (s.ContactName != null && s.ContactName.ToLower().Contains(term)) ||
                (s.Email       != null && s.Email.ToLower().Contains(term)) ||
                (s.Phone       != null && s.Phone.Contains(term)));
        }

        if (request.IsActive.HasValue)
            query = query.Where(s => s.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(s => s.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(s => new SupplierListDto
        {
            Id          = s.Id,
            Name        = s.Name,
            ContactName = s.ContactName,
            Email       = s.Email,
            Phone       = s.Phone,
            GstNumber   = s.GstNumber,
            IsActive    = s.IsActive,
            TotalOrders = s.PurchaseOrders.Count,
            CreatedAt   = s.CreatedAt
        }).ToList();

        return PagedResult<SupplierListDto>.Create(
            dtos, total, request.PageNumber, request.PageSize);
    }
}
