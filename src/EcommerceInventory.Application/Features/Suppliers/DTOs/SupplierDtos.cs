namespace EcommerceInventory.Application.Features.Suppliers.DTOs;

public record SupplierDto(
    Guid Id,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    AddressDto? Address,
    string? GstNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record AddressDto(
    string? Street,
    string? City,
    string? State,
    string? Pincode,
    string? Country);

public record CreateSupplierDto(
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Street,
    string? City,
    string? State,
    string? Pincode,
    string? Country,
    string? GstNumber);

public record UpdateSupplierDto(
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Street,
    string? City,
    string? State,
    string? Pincode,
    string? Country,
    string? GstNumber);
