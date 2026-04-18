namespace EcommerceInventory.Application.Features.Warehouses.DTOs;

// ── Shared base — eliminates duplication ────────────────────
public abstract class WarehouseBaseDto
{
    public Guid    Id              { get; set; }
    public string  Name            { get; set; } = string.Empty;
    public string  Code            { get; set; } = string.Empty;
    public bool    IsActive        { get; set; }
    public string  Status          { get; set; } = "Active";
    public string? Phone           { get; set; }
    public string? Email           { get; set; }
    public int?    Capacity        { get; set; }
    public double? Utilization     { get; set; }
    public string? ManagerName     { get; set; }
    public int     TotalStockLines { get; set; }
    public int     Version         { get; set; }
    public DateTime CreatedAt      { get; set; }
}

// ── Full DTO (GetById, Create, Update) ──────────────────────
public class WarehouseDto : WarehouseBaseDto
{
    public Guid?       ManagerId     { get; set; }
    public AddressDto? Address       { get; set; }
    public string?     AddressString { get; set; }
    public DateTime    UpdatedAt     { get; set; }
}

// ── List DTO (GetAll) ───────────────────────────────────────
public class WarehouseListDto : WarehouseBaseDto
{
    public string? AddressString { get; set; }
}

// ── Address DTO ─────────────────────────────────────────────
public class AddressDto
{
    public string Street  { get; set; } = string.Empty;
    public string City    { get; set; } = string.Empty;
    public string State   { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string Country { get; set; } = "India";
}
