namespace EcommerceInventory.Domain.Exceptions;

/// <summary>
/// Thrown when an unauthorized action is attempted
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
