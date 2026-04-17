using EcommerceInventory.Application.Features.Products.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<ProductDto>
{
    public Guid Id { get; set; }
}
