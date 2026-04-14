using EcommerceInventory.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EcommerceInventory.API.Authorization;

/// <summary>
/// Authorization handler that checks if the current user has the required permission
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Check if user has the required permission claim
        var hasPermission = context.User.HasClaim(c =>
            c.Type == "permission" &&
            string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
