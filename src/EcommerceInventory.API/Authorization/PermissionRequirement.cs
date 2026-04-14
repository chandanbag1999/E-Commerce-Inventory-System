using Microsoft.AspNetCore.Authorization;

namespace EcommerceInventory.API.Authorization;

/// <summary>
/// Authorization requirement that checks for a specific permission
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
