using EIVMS.Domain.Enums;

namespace EIVMS.Application.Modules.UserManagement.DTOs.Address;

public class AddressResponseDto
{
    public Guid Id { get; set; }
    public AddressType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsDefault { get; set; }
    public bool IsValidated { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FormattedAddress => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}