using EIVMS.Domain.Entities.Identity;
using EIVMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EIVMS.Infrastructure.Seeders;

public static class RolePermissionSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
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

        var existingPermissions = await context.Permissions
            .ToDictionaryAsync(permission => permission.Name, StringComparer.OrdinalIgnoreCase);

        var missingPermissions = permissions.Values
            .Where(permission => !existingPermissions.ContainsKey(permission.Name))
            .ToList();

        if (missingPermissions.Count > 0)
        {
            await context.Permissions.AddRangeAsync(missingPermissions);
            await context.SaveChangesAsync();
        }

        var allPermissions = await context.Permissions
            .ToDictionaryAsync(permission => permission.Name, StringComparer.OrdinalIgnoreCase);

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

        var deliveryRole = Role.Create("Delivery", "Delivery operations and shipment visibility");
        var deliveryPermissions = new[]
        {
            "order:read",
            "dashboard:view",
            "report:view"
        };

        var desiredRoles = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase)
        {
            [superAdminRole.Name] = superAdminRole,
            [adminRole.Name] = adminRole,
            [inventoryManagerRole.Name] = inventoryManagerRole,
            [salesManagerRole.Name] = salesManagerRole,
            [staffRole.Name] = staffRole,
            [deliveryRole.Name] = deliveryRole,
        };

        var existingRoles = await context.Roles
            .ToDictionaryAsync(role => role.Name, StringComparer.OrdinalIgnoreCase);

        var missingRoles = desiredRoles.Values
            .Where(role => !existingRoles.ContainsKey(role.Name))
            .ToList();

        if (missingRoles.Count > 0)
        {
            await context.Roles.AddRangeAsync(missingRoles);
            await context.SaveChangesAsync();
        }

        var allRoles = await context.Roles
            .ToDictionaryAsync(role => role.Name, StringComparer.OrdinalIgnoreCase);

        var desiredRolePermissions = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["SuperAdmin"] = superAdminPermissions,
            ["Admin"] = adminPermissions,
            ["InventoryManager"] = inventoryManagerPermissions,
            ["SalesManager"] = salesManagerPermissions,
            ["Staff"] = staffPermissions,
            ["Delivery"] = deliveryPermissions,
        };

        var existingRolePermissions = await context.RolePermissions
            .Select(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId })
            .ToListAsync();

        var missingRolePermissions = new List<RolePermission>();

        foreach (var (roleName, permissionNames) in desiredRolePermissions)
        {
            if (!allRoles.TryGetValue(roleName, out var role)) continue;

            foreach (var permissionName in permissionNames)
            {
                if (!allPermissions.TryGetValue(permissionName, out var permission)) continue;

                var alreadyLinked = existingRolePermissions.Any(existing =>
                    existing.RoleId == role.Id && existing.PermissionId == permission.Id);

                if (!alreadyLinked)
                {
                    missingRolePermissions.Add(RolePermission.Create(role.Id, permission.Id));
                }
            }
        }

        if (missingRolePermissions.Count > 0)
        {
            await context.RolePermissions.AddRangeAsync(missingRolePermissions);
            await context.SaveChangesAsync();
        }

        Console.WriteLine("✅ Roles and Permissions seeded successfully!");
        Console.WriteLine($"   Roles: SuperAdmin, Admin, InventoryManager, SalesManager, Delivery, Staff");
        Console.WriteLine($"   Permissions: {allPermissions.Count} total");
    }
}
