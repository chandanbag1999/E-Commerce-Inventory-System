using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Exceptions;
using EcommerceInventory.Domain.ValueObjects;

namespace EcommerceInventory.Domain.Entities;

public class Supplier : BaseEntity, ISoftDelete
{
    public string   Name        { get; private set; } = string.Empty;
    public string?  ContactName { get; private set; }
    public string?  Email       { get; private set; }
    public string?  Phone       { get; private set; }
    public Address? Address     { get; private set; }
    public string?  GstNumber   { get; private set; }
    public bool     IsActive    { get; private set; } = true;
    public DateTime? DeletedAt  { get; set; }
    public bool     IsDeleted   => DeletedAt.HasValue;

    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    protected Supplier() { }

    public static Supplier Create(string name, string? contactName = null,
                                   string? email = null, string? phone = null,
                                   Address? address = null, string? gstNumber = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name is required.");

        return new Supplier
        {
            Name        = name.Trim(),
            ContactName = contactName?.Trim(),
            Email       = email?.Trim().ToLower(),
            Phone       = phone?.Trim(),
            Address     = address,
            GstNumber   = gstNumber?.Trim(),
            IsActive    = true
        };
    }

    public void Update(string name, string? contactName, string? email,
                       string? phone, Address? address, string? gstNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name is required.");

        Name        = name.Trim();
        ContactName = contactName?.Trim();
        Email       = email?.Trim().ToLower();
        Phone       = phone?.Trim();
        Address     = address;
        GstNumber   = gstNumber?.Trim();
        UpdatedAt   = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive  = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive  = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}