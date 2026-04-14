namespace EcommerceInventory.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }
}
