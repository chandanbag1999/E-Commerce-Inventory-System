using AutoMapper;
using EcommerceInventory.Application.Features.Auth.DTOs;
using EcommerceInventory.Application.Features.Users.DTOs;
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

        // User mappings
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name)));
        
        CreateMap<User, UserListDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name)));

        // Future mappings will be added here
        // CreateMap<Category, CategoryDto>();
        // CreateMap<Product, ProductDto>();
        // CreateMap<Warehouse, WarehouseDto>();
        // CreateMap<Supplier, SupplierDto>();
    }
}
