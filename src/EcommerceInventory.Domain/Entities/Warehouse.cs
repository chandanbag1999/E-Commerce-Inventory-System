using EcommerceInventory.Domain.Common;
using EcommerceInventory.Domain.Exceptions;
using EcommerceInventory.Domain.ValueObjects;

namespace EcommerceInventory.Domain.Entities;

/// <summary>
/// Warehouse entity - physical storage location
/// </summary>
public class Warehouse : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Address? Address { get; set; }
    public Guid? ManagerId { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public User? Manager { get; set; }
    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();

    /// <summary>
    /// Factory method to create a new warehouse
    /// </summary>
    public static Warehouse Create(
        string name,
        string code,
        Address? address = null,
        Guid? managerId = null,
        string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Warehouse name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Warehouse code cannot be empty");

        return new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Code = code.Trim().ToUpper(),
            Address = address,
            ManagerId = managerId,
            Phone = phone?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates warehouse information
    /// </summary>
    public void Update(
        string name,
        string code,
        Address? address = null,
        Guid? managerId = null,
        string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Warehouse name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Warehouse code cannot be empty");

        Name = name.Trim();
        Code = code.Trim().ToUpper();
        Address = address;
        ManagerId = managerId;
        Phone = phone?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft deletes the warehouse
    /// </summary>
    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
