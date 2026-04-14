namespace EcommerceInventory.Domain.Exceptions;

/// <summary>
/// Thrown when a requested entity is not found
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object id) 
        : base($"Entity '{entityName}' with ID '{id}' was not found.")
    {
    }
}
