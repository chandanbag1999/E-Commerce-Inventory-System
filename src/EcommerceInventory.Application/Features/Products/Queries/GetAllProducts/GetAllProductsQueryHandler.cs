using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Products.DTOs;
using EcommerceInventory.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler
    : IRequestHandler<GetAllProductsQuery, PagedResult<ProductListDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllProductsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResult<ProductListDto>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _uow.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Stocks)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Sku.ToLower().Contains(term));
        }

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<ProductStatus>(request.Status, out var status))
            query = query.Where(p => p.Status == status);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.UnitPrice >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.UnitPrice <= request.MaxPrice.Value);

        query = request.SortBy.ToLower() switch
        {
            "price"  => request.SortDesc
                ? query.OrderByDescending(p => p.UnitPrice)
                : query.OrderBy(p => p.UnitPrice),
            "sku"    => query.OrderBy(p => p.Sku),
            "name"   => request.SortDesc
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            _        => request.SortDesc
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(p => new ProductListDto
        {
            Id           = p.Id,
            Name         = p.Name,
            Sku          = p.Sku,
            CategoryName = p.Category.Name,
            UnitPrice    = p.UnitPrice,
            CostPrice    = p.CostPrice,
            Status       = p.Status.ToString(),
            PrimaryImage = p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                        ?? p.Images.FirstOrDefault()?.ImageUrl,
            TotalStock   = p.Stocks.Sum(s => s.Quantity),
            CreatedAt    = p.CreatedAt
        }).ToList();

        return PagedResult<ProductListDto>.Create(dtos, total,
            request.PageNumber, request.PageSize);
    }
}
