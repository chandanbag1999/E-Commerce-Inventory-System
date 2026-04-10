using EIVMS.Domain.Common;
using EIVMS.Domain.Enums;

namespace EIVMS.Domain.Entities.UserManagement;

public class Address : BaseEntity
{
    public Guid UserId { get; private set; }
    public AddressType Type { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string? ContactName { get; private set; }
    public string? ContactPhone { get; private set; }
    public bool IsDefault { get; private set; } = false;
    public bool IsDeleted { get; private set; } = false;
    public bool IsValidated { get; private set; } = false;

    public Identity.User User { get; private set; } = null!;

    private Address() { }

    public static Address Create(
        Guid userId,
        AddressType type,
        string label,
        string street,
        string city,
        string state,
        string country,
        string zipCode,
        string? contactName = null,
        string? contactPhone = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required");
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required");
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required");

        return new Address
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Label = label.Trim(),
            Street = street.Trim(),
            City = city.Trim(),
            State = state.Trim(),
            Country = country.Trim(),
            ZipCode = zipCode.Trim(),
            ContactName = contactName,
            ContactPhone = contactPhone,
            IsDefault = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string label,
        string street,
        string city,
        string state,
        string country,
        string zipCode,
        string? contactName,
        string? contactPhone)
    {
        Label = label;
        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
        ContactName = contactName;
        ContactPhone = contactPhone;
        IsValidated = false;
        SetUpdatedAt();
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        SetUpdatedAt();
    }

    public void UnsetDefault()
    {
        IsDefault = false;
        SetUpdatedAt();
    }

    public void SetGeoCoordinates(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
        SetUpdatedAt();
    }

    public void MarkAsValidated()
    {
        IsValidated = true;
        SetUpdatedAt();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        IsDefault = false;
        SetUpdatedAt();
    }

    public double? DistanceFrom(double latitude, double longitude)
    {
        if (!Latitude.HasValue || !Longitude.HasValue)
            return null;

        var addressCoord = new ValueObjects.GeoCoordinate(Latitude.Value, Longitude.Value);
        var otherCoord = new ValueObjects.GeoCoordinate(latitude, longitude);
        return addressCoord.DistanceTo(otherCoord);
    }
}