using EcommerceInventory.Application.Features.Products.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Products.Queries.GetProductBySku;

public class GetProductBySkuQuery : IRequest<ProductDto>
{
    public string Sku { get; set; } = string.Empty;
}
