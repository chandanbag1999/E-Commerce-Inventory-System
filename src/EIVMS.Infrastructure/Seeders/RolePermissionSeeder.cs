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
            ["inventory:receive"] = Permission.Create("inventory:receive", "inventory", "receive", "Receive stock"),
            ["inventory:dispatch"] = Permission.Create("inventory:dispatch", "inventory", "dispatch", "Dispatch stock"),
            ["order:create"] = Permission.Create("order:create", "order", "create", "Create orders"),
            ["order:read"]   = Permission.Create("order:read",   "order", "read",   "View orders"),
            ["order:update"] = Permission.Create("order:update", "order", "update", "Update orders"),
            ["order:cancel"] = Permission.Create("order:cancel", "order", "cancel", "Cancel orders"),
            ["order:fulfill"] = Permission.Create("order:fulfill", "order", "fulfill", "Fulfill orders"),
            ["user:manage"] = Permission.Create("user:manage", "user", "manage", "Manage users"),
            ["user:create"] = Permission.Create("user:create", "user", "create", "Create users"),
            ["user:view"]   = Permission.Create("user:view",   "user", "view",   "View users"),
            ["user:update"] = Permission.Create("user:update", "user", "update", "Update users"),
            ["user:delete"] = Permission.Create("user:delete", "user", "delete", "Delete users"),
            ["role:manage"] = Permission.Create("role:manage", "role", "manage", "Manage roles"),
            ["report:generate"] = Permission.Create("report:generate", "report", "generate", "Generate reports"),
            ["report:view"]     = Permission.Create("report:view",     "report", "view",     "View reports"),
            ["settings:manage"] = Permission.Create("settings:manage", "settings", "manage", "Manage system settings"),
            ["dashboard:view"] = Permission.Create("dashboard:view", "dashboard", "view", "View dashboard"),
        };

        await context.Permissions.AddRangeAsync(permissions.Values);

        var superAdminRole = Role.Create("SuperAdmin", "Full system access - can manage all users and system");
        var superAdminPermissions = permissions.Keys.ToList();

        var adminRole = Role.Create("Admin", "System administration - user and settings management");
        var adminPermissions = new[]
        {
            "product:create", "product:read", "product:update", "product:delete",
            "inventory:manage", "inventory:view", "inventory:receive", "inventory:dispatch",
            "order:create", "order:read", "order:update", "order:cancel", "order:fulfill",
            "user:create", "user:view", "user:update",
            "report:generate", "report:view",
            "dashboard:view", "settings:manage"
        };

        var inventoryManagerRole = Role.Create("InventoryManager", "Warehouse and inventory management");
        var inventoryManagerPermissions = new[]
        {
            "product:read",
            "inventory:manage", "inventory:view", "inventory:receive", "inventory:dispatch",
            "order:read", "order:update",
            "report:generate", "report:view",
            "dashboard:view"
        };

        var salesManagerRole = Role.Create("SalesManager", "Sales and order management");
        var salesManagerPermissions = new[]
        {
            "product:create", "product:read", "product:update",
            "inventory:view",
            "order:create", "order:read", "order:update", "order:cancel", "order:fulfill",
            "report:generate", "report:view",
            "dashboard:view"
        };

        var staffRole = Role.Create("Staff", "Operational tasks - inventory and orders");
        var staffPermissions = new[]
        {
            "product:read",
            "inventory:view", "inventory:receive", "inventory:dispatch",
            "order:read", "order:update",
            "dashboard:view"
        };

        await context.Roles.AddRangeAsync(superAdminRole, adminRole, inventoryManagerRole, salesManagerRole, staffRole);
        await context.SaveChangesAsync();

        var rolePermissions = new List<RolePermission>();

        foreach (var permKey in superAdminPermissions)
        {
            rolePermissions.Add(RolePermission.Create(superAdminRole.Id, permissions[permKey].Id));
        }

        foreach (var permKey in adminPermissions)
        {
            rolePermissions.Add(RolePermission.Create(adminRole.Id, permissions[permKey].Id));
        }

        foreach (var permKey in inventoryManagerPermissions)
        {
            rolePermissions.Add(RolePermission.Create(inventoryManagerRole.Id, permissions[permKey].Id));
        }

        foreach (var permKey in salesManagerPermissions)
        {
            rolePermissions.Add(RolePermission.Create(salesManagerRole.Id, permissions[permKey].Id));
        }

        foreach (var permKey in staffPermissions)
        {
            rolePermissions.Add(RolePermission.Create(staffRole.Id, permissions[permKey].Id));
        }

        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Roles and Permissions seeded successfully!");
        Console.WriteLine($"   Roles: SuperAdmin, Admin, InventoryManager, SalesManager, Staff");
        Console.WriteLine($"   Permissions: {permissions.Count} total");
    }
}