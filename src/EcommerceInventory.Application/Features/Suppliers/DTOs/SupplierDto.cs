namespace EcommerceInventory.Application.Features.Suppliers.DTOs;

public class SupplierDto
{
    public Guid    Id          { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email       { get; set; }
    public string? Phone       { get; set; }
    public string? GstNumber   { get; set; }
    public bool    IsActive    { get; set; }
    public SupplierAddressDto? Address { get; set; }
    public int     TotalOrders { get; set; }
    public DateTime CreatedAt  { get; set; }
    public DateTime UpdatedAt  { get; set; }
}

public class SupplierListDto
{
    public Guid    Id          { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email       { get; set; }
    public string? Phone       { get; set; }
    public string? GstNumber   { get; set; }
    public bool    IsActive    { get; set; }
    public int     TotalOrders { get; set; }
    public DateTime CreatedAt  { get; set; }
}

public class SupplierAddressDto
{
    public string? Street  { get; set; }
    public string? City    { get; set; }
    public string? State   { get; set; }
    public string? Pincode { get; set; }
    public string? Country { get; set; }
}
