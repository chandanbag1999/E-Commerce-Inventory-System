using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Exceptions;
using EcommerceInventory.Domain.ValueObjects;

namespace EcommerceInventory.Domain.Entities;

public class Warehouse : AuditableEntity, ISoftDelete
{
    public string   Name      { get; private set; } = string.Empty;
    public string   Code      { get; private set; } = string.Empty;
    public Address? Address   { get; private set; }
    public Guid?    ManagerId { get; private set; }
    public string?  Phone     { get; private set; }
    public string?  Email     { get; private set; }
    public int?     Capacity  { get; private set; }
    public bool     IsActive  { get; private set; } = true;
    public int      Version   { get; set; } = 0;
    public DateTime? DeletedAt { get; set; }
    public bool     IsDeleted => DeletedAt.HasValue;

    public User?             Manager { get; set; }
    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();

    protected Warehouse() { }

    public static Warehouse Create(
        string   name,
        string   code,
        Address? address   = null,
        Guid?    managerId = null,
        string?  phone     = null,
        string?  email     = null,
        int?     capacity  = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Warehouse name is required.");
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Warehouse code is required.");

        return new Warehouse
        {
            Name      = name.Trim(),
            Code      = code.Trim().ToUpper(),
            Address   = address,
            ManagerId = managerId,
            Phone     = phone?.Trim(),
            Email     = email?.Trim().ToLower(),
            Capacity  = capacity,
            IsActive  = true
        };
    }

    public void Update(
        string   name,
        Address? address,
        Guid?    managerId,
        string?  phone,
        string?  email,
        int?     capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Warehouse name is required.");

        Name      = name.Trim();
        Address   = address;
        ManagerId = managerId;
        Phone     = phone?.Trim();
        Email     = email?.Trim().ToLower();
        Capacity  = capacity;
        Version++;
        UpdatedAt = DateTime.UtcNow;
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
        IsActive  = false;          // #17: deactivate on delete
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
