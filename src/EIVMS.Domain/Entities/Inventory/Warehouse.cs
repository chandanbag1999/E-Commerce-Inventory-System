using EIVMS.Domain.Common;
using EIVMS.Domain.Enums.Inventory;

namespace EIVMS.Domain.Entities.Inventory;

public class Warehouse : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string PinCode { get; private set; } = string.Empty;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? ContactEmail { get; private set; }
    public int? MaxCapacity { get; private set; }
    public int? CurrentCapacity { get; private set; }
    public WarehouseStatus Status { get; private set; } = WarehouseStatus.Active;
    public bool IsDefault { get; private set; } = false;
    public bool IsDeleted { get; private set; } = false;
    public int Priority { get; private set; } = 1;

    public ICollection<InventoryItem> InventoryItems { get; private set; } = new List<InventoryItem>();

    private Warehouse() { }

    public static Warehouse Create(
        string name,
        string code,
        string address,
        string city,
        string state,
        string country,
        string pinCode,
        double latitude,
        double longitude,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name is required");
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Warehouse code is required");

        return new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Code = code.ToUpperInvariant().Trim(),
            Address = address.Trim(),
            City = city.Trim(),
            State = state.Trim(),
            Country = country.Trim(),
            PinCode = pinCode.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            IsDefault = isDefault,
            Status = WarehouseStatus.Active,
            Priority = 1,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string name,
        string? description,
        string address,
        string? contactPerson,
        string? contactPhone,
        string? contactEmail,
        int priority)
    {
        Name = name.Trim();
        Description = description;
        Address = address.Trim();
        ContactPerson = contactPerson;
        ContactPhone = contactPhone;
        ContactEmail = contactEmail;
        Priority = priority;
        SetUpdatedAt();
    }

    public void UpdateGeoLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        SetUpdatedAt();
    }

    public void SetCapacity(int maxCapacity)
    {
        MaxCapacity = maxCapacity;
        SetUpdatedAt();
    }

    public void UnsetDefault() { IsDefault = false; SetUpdatedAt(); }

    public void Activate() { Status = WarehouseStatus.Active; SetUpdatedAt(); }
    public void Deactivate() { Status = WarehouseStatus.Inactive; SetUpdatedAt(); }
    public void SetUnderMaintenance() { Status = WarehouseStatus.UnderMaintenance; SetUpdatedAt(); }
    public void SetAsDefault() { IsDefault = true; SetUpdatedAt(); }
    public void SoftDelete() { IsDeleted = true; Status = WarehouseStatus.Closed; SetUpdatedAt(); }

    public double DistanceTo(double lat, double lon)
    {
        const double R = 6371;
        var dLat = ToRadians(lat - Latitude);
        var dLon = ToRadians(lon - Longitude);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(lat)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double deg) => deg * Math.PI / 180;
    public bool IsOperational => Status == WarehouseStatus.Active && !IsDeleted;
}