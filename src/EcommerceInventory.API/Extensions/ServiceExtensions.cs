using EcommerceInventory.API.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceInventory.API.Extensions;

/// <summary>
/// Extension methods to register API layer services (Authorization policies, etc.)
/// Note: JWT Authentication is already registered in Infrastructure.DependencyInjection
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Register the permission authorization handler
    /// </summary>
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        return services;
    }

    /// <summary>
    /// Register all permission-based authorization policies in the service container
    /// Each permission gets a policy named "Permission:{PermissionName}"
    /// </summary>
    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services, IEnumerable<string> permissions)
    {
        foreach (var permission in permissions)
        {
            var policyName = $"Permission:{permission}";
            services.AddAuthorizationBuilder().AddPolicy(policyName, policy =>
            {
                policy.Requirements.Add(new PermissionRequirement(permission));
            });
        }

        // SuperAdmin bypass policy — SuperAdmin role has access to everything
        services.AddAuthorizationBuilder().AddPolicy("SuperAdmin", policy =>
        {
            policy.RequireRole("SuperAdmin");
        });

        return services;
    }
}
