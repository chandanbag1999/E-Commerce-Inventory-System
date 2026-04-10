using EIVMS.Domain.Enums;

namespace EIVMS.Application.Modules.UserManagement.DTOs.Address;

public class CreateAddressDto
{
    public AddressType Type { get; set; } = AddressType.Home;
    public string Label { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public bool SetAsDefault { get; set; } = false;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}