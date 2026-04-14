using Microsoft.AspNetCore.Authorization;

namespace EcommerceInventory.API.Authorization;

/// <summary>
/// Attribute to require a specific permission for an endpoint
/// Usage: [HasPermission("Products.View")]
/// </summary>
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
        : base(policy: $"Permission:{permission}")
    {
    }
}
