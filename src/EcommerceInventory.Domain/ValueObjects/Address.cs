namespace EcommerceInventory.Domain.ValueObjects;

public class Address
{
    public string Street  { get; set; } = string.Empty;
    public string City    { get; set; } = string.Empty;
    public string State   { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string Country { get; set; } = "India";

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
}