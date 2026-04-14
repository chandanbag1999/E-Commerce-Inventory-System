using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Products.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Products.Queries.GetAllProducts;

public record GetAllProductsQuery : IRequest<Result<PagedResult<ProductListDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string SortBy { get; set; } = "Name";
    public bool SortDesc { get; set; } = false;
}

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, Result<PagedResult<ProductListDto>>>
{
    private readonly IRepository<Product> _productRepository;

    public GetAllProductsHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PagedResult<ProductListDto>>> Handle(GetAllProductsQuery request, CancellationToken ct)
    {
        var query = _productRepository.Query()
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsPrimary).Take(1))
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = $"%{request.SearchTerm}%";
            query = query.Where(p => EF.Functions.Like(p.Name, term) || EF.Functions.Like(p.Sku, term));
        }

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId);

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ProductStatus>(request.Status, true, out var status))
            query = query.Where(p => p.Status == status);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.UnitPrice >= request.MinPrice);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.UnitPrice <= request.MaxPrice);

        query = request.SortBy?.ToLower() switch
        {
            "price" when !request.SortDesc => query.OrderBy(p => p.UnitPrice),
            "price" when request.SortDesc => query.OrderByDescending(p => p.UnitPrice),
            "createdat" when !request.SortDesc => query.OrderBy(p => p.CreatedAt),
            "createdat" when request.SortDesc => query.OrderByDescending(p => p.CreatedAt),
            "sku" => query.OrderBy(p => p.Sku),
            _ => request.SortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync(ct);
        var products = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var dtos = products.Select(p => new ProductListDto(
            p.Id,
            p.Name,
            p.Sku,
            p.Category.Name,
            p.UnitPrice,
            p.Status.ToString(),
            p.Images.FirstOrDefault()?.ImageUrl,
            p.CreatedAt
        )).ToList();

        return Result<PagedResult<ProductListDto>>.SuccessResult(new PagedResult<ProductListDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}