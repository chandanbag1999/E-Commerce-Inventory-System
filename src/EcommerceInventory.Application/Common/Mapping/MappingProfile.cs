using AutoMapper;
using EcommerceInventory.Application.Features.Auth.DTOs;
using EcommerceInventory.Domain.Entities;

namespace EcommerceInventory.Application.Common.Mapping;

/// <summary>
/// AutoMapper configuration for all entity-to-DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Auth mappings
        CreateMap<User, RegisterResponseDto>();
        CreateMap<User, UserInfoDto>();

        // Future mappings will be added here
        // CreateMap<Category, CategoryDto>();
        // CreateMap<Product, ProductDto>();
        // CreateMap<Warehouse, WarehouseDto>();
        // CreateMap<Supplier, SupplierDto>();
    }
}
