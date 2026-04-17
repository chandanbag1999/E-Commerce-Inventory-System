using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Suppliers.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Suppliers.Queries.GetAllSuppliers;

public class GetAllSuppliersQuery : IRequest<PagedResult<SupplierListDto>>
{
    public int     PageNumber  { get; set; } = 1;
    public int     PageSize    { get; set; } = 20;
    public string? SearchTerm  { get; set; }
    public bool?   IsActive    { get; set; }
}
