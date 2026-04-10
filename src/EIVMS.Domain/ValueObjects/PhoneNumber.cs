using System.Text.RegularExpressions;

namespace EIVMS.Domain.ValueObjects;

public class PhoneNumber
{
    public string CountryCode { get; }
    public string Number { get; }
    public string FullNumber => $"{CountryCode}{Number}";

    private PhoneNumber() { }

    public PhoneNumber(string countryCode, string number)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code is required");

        if (!countryCode.StartsWith("+"))
            throw new ArgumentException("Country code must start with +");

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Phone number is required");

        if (!Regex.IsMatch(number, @"^\d{7,15}$"))
            throw new ArgumentException("Invalid phone number format");

        CountryCode = countryCode;
        Number = number;
    }

    public static PhoneNumber Parse(string fullNumber)
    {
        if (string.IsNullOrWhiteSpace(fullNumber))
            throw new ArgumentException("Phone number cannot be empty");

        var match = Regex.Match(fullNumber, @"^(\+\d{1,4})(\d{7,15})$");
        if (!match.Success)
            throw new ArgumentException($"Invalid phone number: {fullNumber}");

        return new PhoneNumber(match.Groups[1].Value, match.Groups[2].Value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PhoneNumber other) return false;
        return FullNumber == other.FullNumber;
    }

    public override int GetHashCode() => FullNumber.GetHashCode();

    public override string ToString() => FullNumber;
}