using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds 8 roles, 40+ permissions, and their assignments
/// MVP: Roles are seeded only, no dynamic CRUD API (Phase 2)
/// </summary>
public static class RolePermissionSeed
{
    public static void Seed(ModelBuilder builder)
    {
        // ── Role IDs (fixed GUIDs for consistency)
        var superAdminId = Guid.Parse("11111111-0000-0000-0000-000000000001");
        var adminId = Guid.Parse("11111111-0000-0000-0000-000000000002");
        var inventoryManagerId = Guid.Parse("11111111-0000-0000-0000-000000000003");
        var warehouseManagerId = Guid.Parse("11111111-0000-0000-0000-000000000004");
        var purchaseManagerId = Guid.Parse("11111111-0000-0000-0000-000000000005");
        var salesManagerId = Guid.Parse("11111111-0000-0000-0000-000000000006");
        var accountantId = Guid.Parse("11111111-0000-0000-0000-000000000007");
        var viewerId = Guid.Parse("11111111-0000-0000-0000-000000000008");

        // ── Seed Roles (with hardcoded dates for deterministic migrations)
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        builder.Entity<Role>().HasData(
            new Role { Id = superAdminId, Name = "SuperAdmin", HierarchyLevel = 1, IsSystemRole = true, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = adminId, Name = "Admin", HierarchyLevel = 2, IsSystemRole = true, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = inventoryManagerId, Name = "InventoryManager", HierarchyLevel = 3, IsSystemRole = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = warehouseManagerId, Name = "WarehouseManager", HierarchyLevel = 4, IsSystemRole = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = purchaseManagerId, Name = "PurchaseManager", HierarchyLevel = 5, IsSystemRole = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = salesManagerId, Name = "SalesManager", HierarchyLevel = 6, IsSystemRole = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = accountantId, Name = "Accountant", HierarchyLevel = 7, IsSystemRole = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = viewerId, Name = "Viewer", HierarchyLevel = 10, IsSystemRole = false, CreatedAt = seedDate, UpdatedAt = seedDate }
        );

        // ── Permission IDs
        var permUsersView = Guid.Parse("21111111-0000-0000-0000-000000000001");
        var permUsersCreate = Guid.Parse("21111111-0000-0000-0000-000000000002");
        var permUsersEdit = Guid.Parse("21111111-0000-0000-0000-000000000003");
        var permUsersDelete = Guid.Parse("21111111-0000-0000-0000-000000000004");
        var permUsersAssignRole = Guid.Parse("21111111-0000-0000-0000-000000000005");

        var permRolesView = Guid.Parse("21111111-0000-0000-0000-000000000010");

        var permCategoriesView = Guid.Parse("21111111-0000-0000-0000-000000000020");
        var permCategoriesCreate = Guid.Parse("21111111-0000-0000-0000-000000000021");
        var permCategoriesEdit = Guid.Parse("21111111-0000-0000-0000-000000000022");
        var permCategoriesDelete = Guid.Parse("21111111-0000-0000-0000-000000000023");

        var permProductsView = Guid.Parse("21111111-0000-0000-0000-000000000030");
        var permProductsCreate = Guid.Parse("21111111-0000-0000-0000-000000000031");
        var permProductsEdit = Guid.Parse("21111111-0000-0000-0000-000000000032");
        var permProductsDelete = Guid.Parse("21111111-0000-0000-0000-000000000033");

        var permWarehousesView = Guid.Parse("21111111-0000-0000-0000-000000000040");
        var permWarehousesCreate = Guid.Parse("21111111-0000-0000-0000-000000000041");
        var permWarehousesEdit = Guid.Parse("21111111-0000-0000-0000-000000000042");
        var permWarehousesDelete = Guid.Parse("21111111-0000-0000-0000-000000000043");

        var permStocksView = Guid.Parse("21111111-0000-0000-0000-000000000050");
        var permStocksAdjust = Guid.Parse("21111111-0000-0000-0000-000000000051");

        var permSuppliersView = Guid.Parse("21111111-0000-0000-0000-000000000060");
        var permSuppliersCreate = Guid.Parse("21111111-0000-0000-0000-000000000061");
        var permSuppliersEdit = Guid.Parse("21111111-0000-0000-0000-000000000062");
        var permSuppliersDelete = Guid.Parse("21111111-0000-0000-0000-000000000063");

        var permPurchaseOrdersView = Guid.Parse("21111111-0000-0000-0000-000000000070");
        var permPurchaseOrdersCreate = Guid.Parse("21111111-0000-0000-0000-000000000071");
        var permPurchaseOrdersApprove = Guid.Parse("21111111-0000-0000-0000-000000000072");
        var permPurchaseOrdersReceive = Guid.Parse("21111111-0000-0000-0000-000000000073");
        var permPurchaseOrdersCancel = Guid.Parse("21111111-0000-0000-0000-000000000074");

        var permSalesOrdersView = Guid.Parse("21111111-0000-0000-0000-000000000080");
        var permSalesOrdersCreate = Guid.Parse("21111111-0000-0000-0000-000000000081");
        var permSalesOrdersApprove = Guid.Parse("21111111-0000-0000-0000-000000000082");
        var permSalesOrdersShip = Guid.Parse("21111111-0000-0000-0000-000000000083");
        var permSalesOrdersDeliver = Guid.Parse("21111111-0000-0000-0000-000000000084");
        var permSalesOrdersCancel = Guid.Parse("21111111-0000-0000-0000-000000000085");

        // ── Seed PERMISSIONS (41 total)
        builder.Entity<Permission>().HasData(
            // Users (5)
            new Permission { Id = permUsersView, Name = "Users.View", Module = "Users", Description = "View users", CreatedAt = seedDate },
            new Permission { Id = permUsersCreate, Name = "Users.Create", Module = "Users", Description = "Create users", CreatedAt = seedDate },
            new Permission { Id = permUsersEdit, Name = "Users.Edit", Module = "Users", Description = "Edit users", CreatedAt = seedDate },
            new Permission { Id = permUsersDelete, Name = "Users.Delete", Module = "Users", Description = "Delete users", CreatedAt = seedDate },
            new Permission { Id = permUsersAssignRole, Name = "Users.AssignRole", Module = "Users", Description = "Assign/revoke roles", CreatedAt = seedDate },
            
            // Roles (1)
            new Permission { Id = permRolesView, Name = "Roles.View", Module = "Roles", Description = "View roles", CreatedAt = seedDate },
            
            // Categories (4)
            new Permission { Id = permCategoriesView, Name = "Categories.View", Module = "Categories", Description = "View categories", CreatedAt = seedDate },
            new Permission { Id = permCategoriesCreate, Name = "Categories.Create", Module = "Categories", Description = "Create categories", CreatedAt = seedDate },
            new Permission { Id = permCategoriesEdit, Name = "Categories.Edit", Module = "Categories", Description = "Edit categories", CreatedAt = seedDate },
            new Permission { Id = permCategoriesDelete, Name = "Categories.Delete", Module = "Categories", Description = "Delete categories", CreatedAt = seedDate },
            
            // Products (4)
            new Permission { Id = permProductsView, Name = "Products.View", Module = "Products", Description = "View products", CreatedAt = seedDate },
            new Permission { Id = permProductsCreate, Name = "Products.Create", Module = "Products", Description = "Create products", CreatedAt = seedDate },
            new Permission { Id = permProductsEdit, Name = "Products.Edit", Module = "Products", Description = "Edit products", CreatedAt = seedDate },
            new Permission { Id = permProductsDelete, Name = "Products.Delete", Module = "Products", Description = "Delete products", CreatedAt = seedDate },
            
            // Warehouses (4)
            new Permission { Id = permWarehousesView, Name = "Warehouses.View", Module = "Warehouses", Description = "View warehouses", CreatedAt = seedDate },
            new Permission { Id = permWarehousesCreate, Name = "Warehouses.Create", Module = "Warehouses", Description = "Create warehouses", CreatedAt = seedDate },
            new Permission { Id = permWarehousesEdit, Name = "Warehouses.Edit", Module = "Warehouses", Description = "Edit warehouses", CreatedAt = seedDate },
            new Permission { Id = permWarehousesDelete, Name = "Warehouses.Delete", Module = "Warehouses", Description = "Delete warehouses", CreatedAt = seedDate },
            
            // Stocks (2)
            new Permission { Id = permStocksView, Name = "Stocks.View", Module = "Stocks", Description = "View stock levels", CreatedAt = seedDate },
            new Permission { Id = permStocksAdjust, Name = "Stocks.Adjust", Module = "Stocks", Description = "Adjust stock manually", CreatedAt = seedDate },
            
            // Suppliers (4)
            new Permission { Id = permSuppliersView, Name = "Suppliers.View", Module = "Suppliers", Description = "View suppliers", CreatedAt = seedDate },
            new Permission { Id = permSuppliersCreate, Name = "Suppliers.Create", Module = "Suppliers", Description = "Create suppliers", CreatedAt = seedDate },
            new Permission { Id = permSuppliersEdit, Name = "Suppliers.Edit", Module = "Suppliers", Description = "Edit suppliers", CreatedAt = seedDate },
            new Permission { Id = permSuppliersDelete, Name = "Suppliers.Delete", Module = "Suppliers", Description = "Delete suppliers", CreatedAt = seedDate },
            
            // Purchase Orders (5)
            new Permission { Id = permPurchaseOrdersView, Name = "PurchaseOrders.View", Module = "PurchaseOrders", Description = "View purchase orders", CreatedAt = seedDate },
            new Permission { Id = permPurchaseOrdersCreate, Name = "PurchaseOrders.Create", Module = "PurchaseOrders", Description = "Create/manage purchase orders", CreatedAt = seedDate },
            new Permission { Id = permPurchaseOrdersApprove, Name = "PurchaseOrders.Approve", Module = "PurchaseOrders", Description = "Approve/reject purchase orders", CreatedAt = seedDate },
            new Permission { Id = permPurchaseOrdersReceive, Name = "PurchaseOrders.Receive", Module = "PurchaseOrders", Description = "Receive purchase orders", CreatedAt = seedDate },
            new Permission { Id = permPurchaseOrdersCancel, Name = "PurchaseOrders.Cancel", Module = "PurchaseOrders", Description = "Cancel purchase orders", CreatedAt = seedDate },
            
            // Sales Orders (6)
            new Permission { Id = permSalesOrdersView, Name = "SalesOrders.View", Module = "SalesOrders", Description = "View sales orders", CreatedAt = seedDate },
            new Permission { Id = permSalesOrdersCreate, Name = "SalesOrders.Create", Module = "SalesOrders", Description = "Create/manage sales orders", CreatedAt = seedDate },
            new Permission { Id = permSalesOrdersApprove, Name = "SalesOrders.Approve", Module = "SalesOrders", Description = "Approve sales orders", CreatedAt = seedDate },
            new Permission { Id = permSalesOrdersShip, Name = "SalesOrders.Ship", Module = "SalesOrders", Description = "Ship sales orders", CreatedAt = seedDate },
            new Permission { Id = permSalesOrdersDeliver, Name = "SalesOrders.Deliver", Module = "SalesOrders", Description = "Mark sales orders delivered", CreatedAt = seedDate },
            new Permission { Id = permSalesOrdersCancel, Name = "SalesOrders.Cancel", Module = "SalesOrders", Description = "Cancel sales orders", CreatedAt = seedDate }
        );

        // ── Assign permissions to roles
        var rolePermissions = new List<RolePermission>();

        // SuperAdmin: ALL permissions
        var allPermIds = new[]
        {
            permUsersView, permUsersCreate, permUsersEdit, permUsersDelete, permUsersAssignRole,
            permRolesView,
            permCategoriesView, permCategoriesCreate, permCategoriesEdit, permCategoriesDelete,
            permProductsView, permProductsCreate, permProductsEdit, permProductsDelete,
            permWarehousesView, permWarehousesCreate, permWarehousesEdit, permWarehousesDelete,
            permStocksView, permStocksAdjust,
            permSuppliersView, permSuppliersCreate, permSuppliersEdit, permSuppliersDelete,
            permPurchaseOrdersView, permPurchaseOrdersCreate, permPurchaseOrdersApprove, permPurchaseOrdersReceive, permPurchaseOrdersCancel,
            permSalesOrdersView, permSalesOrdersCreate, permSalesOrdersApprove, permSalesOrdersShip, permSalesOrdersDeliver, permSalesOrdersCancel
        };

        foreach (var permId in allPermIds)
        {
            rolePermissions.Add(new RolePermission { RoleId = superAdminId, PermissionId = permId });
        }

        // Admin: All except delete critical entities
        var adminPermIds = new[]
        {
            permUsersView, permUsersCreate, permUsersEdit, permUsersAssignRole,
            permRolesView,
            permCategoriesView, permCategoriesCreate, permCategoriesEdit, permCategoriesDelete,
            permProductsView, permProductsCreate, permProductsEdit, permProductsDelete,
            permWarehousesView, permWarehousesCreate, permWarehousesEdit, permWarehousesDelete,
            permStocksView, permStocksAdjust,
            permSuppliersView, permSuppliersCreate, permSuppliersEdit, permSuppliersDelete,
            permPurchaseOrdersView, permPurchaseOrdersCreate, permPurchaseOrdersApprove, permPurchaseOrdersReceive, permPurchaseOrdersCancel,
            permSalesOrdersView, permSalesOrdersCreate, permSalesOrdersApprove, permSalesOrdersShip, permSalesOrdersDeliver, permSalesOrdersCancel
        };

        foreach (var permId in adminPermIds)
        {
            rolePermissions.Add(new RolePermission { RoleId = adminId, PermissionId = permId });
        }

        // InventoryManager: Categories, Products, Stocks (view + edit)
        var invPermIds = new[]
        {
            permCategoriesView, permCategoriesCreate, permCategoriesEdit,
            permProductsView, permProductsCreate, permProductsEdit,
            permWarehousesView,
            permStocksView, permStocksAdjust
        };

        foreach (var permId in invPermIds)
        {
            rolePermissions.Add(new RolePermission { RoleId = inventoryManagerId, PermissionId = permId });
        }

        // WarehouseManager: Warehouses, Stocks (view only + adjust)
        var whPermIds = new[]
        {
            permWarehousesView,
            permStocksView, permStocksAdjust
        };

        foreach (var permId in whPermIds)
        {
            rolePermissions.Add(new RolePermission { RoleId = warehouseManagerId, PermissionId = permId });
        }

        // PurchaseManager: Suppliers, Purchase Orders
        var poPermIds = new[]
        {
            permSuppliersView, permSuppliersCreate, permSuppliersEdit,
            permProductsView,
            permWarehousesView,
            permPurchaseOrdersView, permPurchaseOrdersCreate, permPurchaseOrdersApprove, permPurchaseOrdersReceive
        };

        foreach (var permId in poPermIds)
        {
            rolePermissions.Add(new RolePermission { RoleId = purchaseManagerId, PermissionId = permId });
        }

        // SalesManager: Sales Orders, Stocks (view)
        var soPermIds = new[]
        {
            permProductsView,
            permWarehousesView,
            permStocksView,
            permSalesOrdersView, permSalesOrdersCreate, permSalesOrdersApprove, permSalesOrdersShip, permSalesOrdersDeliver
        };

        foreach (var permId in soPermIds)
        {
            rolePermissions.Add(new RolePermission { RoleId = salesManagerId, PermissionId = permId });
        }

        // Accountant: View-only for orders, suppliers, products
        var acctPermIds = new[]
        {
            permProductsView,
            permWarehousesView,
            permStocksView,
            permSuppliersView,
            permPurchaseOrdersView,
            permSalesOrdersView
        };

        foreach (var permId in acctPermIds)
        {
            rolePermissions.Add(new RolePermission { RoleId = accountantId, PermissionId = permId });
        }

        // Viewer: View-only everything
        var viewerPermIds = new[]
        {
            permUsersView, permRolesView,
            permCategoriesView, permProductsView, permWarehousesView, permStocksView,
            permSuppliersView, permPurchaseOrdersView, permSalesOrdersView
        };

        foreach (var permId in viewerPermIds)
        {
            rolePermissions.Add(new RolePermission { RoleId = viewerId, PermissionId = permId });
        }

        builder.Entity<RolePermission>().HasData(rolePermissions);
    }
}
