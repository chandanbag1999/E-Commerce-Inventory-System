using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Exceptions;
using EcommerceInventory.Domain.ValueObjects;

namespace EcommerceInventory.Domain.Entities;

/// <summary>
/// Supplier entity - vendor/supplier information
/// </summary>
public class Supplier : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Address? Address { get; set; }
    public string? GstNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    /// <summary>
    /// Factory method to create a new supplier
    /// </summary>
    public static Supplier Create(
        string name,
        string? contactName = null,
        string? email = null,
        string? phone = null,
        Address? address = null,
        string? gstNumber = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name cannot be empty");

        return new Supplier
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            ContactName = contactName?.Trim(),
            Email = email?.Trim().ToLower(),
            Phone = phone?.Trim(),
            Address = address,
            GstNumber = gstNumber?.Trim().ToUpper(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates supplier information
    /// </summary>
    public void Update(
        string name,
        string? contactName = null,
        string? email = null,
        string? phone = null,
        Address? address = null,
        string? gstNumber = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name cannot be empty");

        Name = name.Trim();
        ContactName = contactName?.Trim();
        Email = email?.Trim().ToLower();
        Phone = phone?.Trim();
        Address = address;
        GstNumber = gstNumber?.Trim().ToUpper();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft deletes the supplier
    /// </summary>
    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
