namespace EcommerceInventory.Application.Features.Warehouses.DTOs;

public record WarehouseDto(
    Guid Id,
    string Name,
    string Code,
    AddressDto? Address,
    Guid? ManagerId,
    string? ManagerName,
    string? Phone,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record AddressDto(
    string? Street,
    string? City,
    string? State,
    string? Pincode,
    string? Country);

public record CreateWarehouseDto(
    string Name,
    string Code,
    string? Street,
    string? City,
    string? State,
    string? Pincode,
    string? Country,
    Guid? ManagerId,
    string? Phone);

public record UpdateWarehouseDto(
    string Name,
    string Code,
    string? Street,
    string? City,
    string? State,
    string? Pincode,
    string? Country,
    Guid? ManagerId,
    string? Phone);
