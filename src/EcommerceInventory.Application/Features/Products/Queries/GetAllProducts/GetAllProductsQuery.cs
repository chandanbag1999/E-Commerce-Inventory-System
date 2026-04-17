using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Products.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsQuery : IRequest<PagedResult<ProductListDto>>
{
    public int     PageNumber  { get; set; } = 1;
    public int     PageSize    { get; set; } = 20;
    public string? SearchTerm  { get; set; }
    public Guid?   CategoryId  { get; set; }
    public string? Status      { get; set; }
    public decimal? MinPrice   { get; set; }
    public decimal? MaxPrice   { get; set; }
    public string  SortBy      { get; set; } = "CreatedAt";
    public bool    SortDesc    { get; set; } = true;
}
