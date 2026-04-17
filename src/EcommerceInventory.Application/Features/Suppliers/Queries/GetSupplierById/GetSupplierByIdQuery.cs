using EcommerceInventory.Application.Features.Suppliers.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Suppliers.Queries.GetSupplierById;

public class GetSupplierByIdQuery : IRequest<SupplierDto>
{
    public Guid Id { get; set; }
}
