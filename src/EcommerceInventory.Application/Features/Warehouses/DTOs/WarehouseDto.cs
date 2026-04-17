using EcommerceInventory.Domain.ValueObjects;

namespace EcommerceInventory.Application.Features.Warehouses.DTOs;

public class WarehouseDto
{
    public Guid     Id        { get; set; }
    public string   Name      { get; set; } = string.Empty;
    public string   Code      { get; set; } = string.Empty;
    public bool     IsActive  { get; set; }
    public string?  Phone     { get; set; }
    public Guid?    ManagerId { get; set; }
    public string?  ManagerName { get; set; }
    public AddressDto? Address { get; set; }
    public int      TotalStockLines { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class WarehouseListDto
{
    public Guid    Id       { get; set; }
    public string  Name     { get; set; } = string.Empty;
    public string  Code     { get; set; } = string.Empty;
    public bool    IsActive { get; set; }
    public string? Phone    { get; set; }
    public string? ManagerName { get; set; }
    public int     TotalStockLines { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AddressDto
{
    public string? Street  { get; set; }
    public string? City    { get; set; }
    public string? State   { get; set; }
    public string? Pincode { get; set; }
    public string? Country { get; set; }
}
