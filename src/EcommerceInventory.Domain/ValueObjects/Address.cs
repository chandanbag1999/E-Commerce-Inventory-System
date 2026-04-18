namespace EcommerceInventory.Domain.ValueObjects;

public class Address : IEquatable<Address>
{
    public string Street  { get; init; } = string.Empty;
    public string City    { get; init; } = string.Empty;
    public string State   { get; init; } = string.Empty;
    public string Pincode { get; init; } = string.Empty;
    public string Country { get; init; } = "India";

    public Address() { }

    public Address(string street, string city, string state,
                   string pincode, string country = "India")
    {
        Street  = street;
        City    = city;
        State   = state;
        Pincode = pincode;
        Country = country;
    }

    public override string ToString() =>
        $"{Street}, {City}, {State} - {Pincode}, {Country}";

    public bool Equals(Address? other)
    {
        if (other is null) return false;
        return Street  == other.Street
            && City    == other.City
            && State   == other.State
            && Pincode == other.Pincode
            && Country == other.Country;
    }

    public override bool Equals(object? obj) => Equals(obj as Address);

    public override int GetHashCode() =>
        HashCode.Combine(Street, City, State, Pincode, Country);
}
