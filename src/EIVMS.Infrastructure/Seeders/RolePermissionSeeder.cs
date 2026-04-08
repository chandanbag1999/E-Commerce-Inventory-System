using EIVMS.Domain.Entities.Identity;
using EIVMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EIVMS.Infrastructure.Seeders;

public static class RolePermissionSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Roles.AnyAsync()) return;

        var permissions = new Dictionary<string, Permission>
        {
            ["product:create"] = Permission.Create("product:create", "product", "create", "Create new products"),
            ["product:read"]   = Permission.Create("product:read",   "product", "read",   "View products"),
            ["product:update"] = Permission.Create("product:update", "product", "update", "Update products"),
            ["product:delete"] = Permission.Create("product:delete", "product", "delete", "Delete products"),
            ["inventory:manage"] = Permission.Create("inventory:manage", "inventory", "manage", "Full inventory management"),
            ["inventory:view"]   = Permission.Create("inventory:view",   "inventory", "view",   "View inventory"),
            ["order:create"] = Permission.Create("order:create", "order", "create", "Create orders"),
            ["order:read"]   = Permission.Create("order:read",   "order", "read",   "View orders"),
            ["order:update"] = Permission.Create("order:update", "order", "update", "Update orders"),
            ["order:cancel"] = Permission.Create("order:cancel", "order", "cancel", "Cancel orders"),
            ["user:manage"] = Permission.Create("user:manage", "user", "manage", "Manage users"),
            ["user:view"]   = Permission.Create("user:view",   "user", "view",   "View users"),
            ["report:generate"] = Permission.Create("report:generate", "report", "generate", "Generate reports"),
            ["report:view"]     = Permission.Create("report:view",     "report", "view",     "View reports"),
        };

        await context.Permissions.AddRangeAsync(permissions.Values);

        var adminRole = Role.Create("Admin", "Full system access - all permissions");
        var adminPermissions = permissions.Keys.ToList();

        var vendorRole = Role.Create("Vendor", "Vendor access - own product and inventory management");
        var vendorPermissions = new[]
        {
            "product:create", "product:read", "product:update", "product:delete",
            "inventory:manage", "inventory:view",
            "order:read",
            "report:generate", "report:view"
        };

        var staffRole = Role.Create("Staff", "Staff access - operational tasks");
        var staffPermissions = new[]
        {
            "product:read",
            "inventory:manage", "inventory:view",
            "order:create", "order:read", "order:update",
            "report:view"
        };

        var customerRole = Role.Create("Customer", "Customer access - order related only");
        var customerPermissions = new[]
        {
            "product:read",
            "order:create", "order:read", "order:cancel"
        };

        await context.Roles.AddRangeAsync(adminRole, vendorRole, staffRole, customerRole);
        await context.SaveChangesAsync();

        var rolePermissions = new List<RolePermission>();

        foreach (var permKey in adminPermissions)
        {
            rolePermissions.Add(RolePermission.Create(adminRole.Id, permissions[permKey].Id));
        }

        foreach (var permKey in vendorPermissions)
        {
            rolePermissions.Add(RolePermission.Create(vendorRole.Id, permissions[permKey].Id));
        }

        foreach (var permKey in staffPermissions)
        {
            rolePermissions.Add(RolePermission.Create(staffRole.Id, permissions[permKey].Id));
        }

        foreach (var permKey in customerPermissions)
        {
            rolePermissions.Add(RolePermission.Create(customerRole.Id, permissions[permKey].Id));
        }

        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Roles and Permissions seeded successfully!");
        Console.WriteLine($"   Roles: Admin, Vendor, Staff, Customer");
        Console.WriteLine($"   Permissions: {permissions.Count} total");
    }
}