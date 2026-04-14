namespace EcommerceInventory.Domain.ValueObjects;

/// <summary>
/// Value object representing a physical address
/// </summary>
public record Address
{
    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string Pincode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;

    public Address() { }

    public Address(string street, string city, string state, string pincode, string country)
    {
        Street = string.IsNullOrWhiteSpace(street) 
            ? throw new ArgumentException("Street cannot be empty", nameof(street)) 
            : street;
        City = string.IsNullOrWhiteSpace(city) 
            ? throw new ArgumentException("City cannot be empty", nameof(city)) 
            : city;
        State = string.IsNullOrWhiteSpace(state) 
            ? throw new ArgumentException("State cannot be empty", nameof(state)) 
            : state;
        Pincode = string.IsNullOrWhiteSpace(pincode) 
            ? throw new ArgumentException("Pincode cannot be empty", nameof(pincode)) 
            : pincode;
        Country = string.IsNullOrWhiteSpace(country) 
            ? throw new ArgumentException("Country cannot be empty", nameof(country)) 
            : country;
    }

    public override string ToString()
    {
        return $"{Street}, {City}, {State} - {Pincode}, {Country}";
    }
}
