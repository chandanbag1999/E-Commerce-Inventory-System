using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Infrastructure.Persistence.Seed;

public static class RolePermissionSeed
{
    public static void Seed(ModelBuilder builder)
    {
        var superAdminId = Guid.Parse("11111111-0000-0000-0000-000000000001");
        var adminId = Guid.Parse("11111111-0000-0000-0000-000000000002");
        var inventoryManagerId = Guid.Parse("11111111-0000-0000-0000-000000000003");
        var warehouseManagerId = Guid.Parse("11111111-0000-0000-0000-000000000004");
        var purchaseManagerId = Guid.Parse("11111111-0000-0000-0000-000000000005");
        var salesManagerId = Guid.Parse("11111111-0000-0000-0000-000000000006");
        var accountantId = Guid.Parse("11111111-0000-0000-0000-000000000007");
        var viewerId = Guid.Parse("11111111-0000-0000-0000-000000000008");

        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.Entity<Role>().HasData(
            new Role { Id = superAdminId, Name = "SuperAdmin", Description = "Full system access", HierarchyLevel = 1, IsSystemRole = true, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = adminId, Name = "Admin", Description = "Administrative access", HierarchyLevel = 2, IsSystemRole = true, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = inventoryManagerId, Name = "InventoryManager", Description = "Manage inventory and products", HierarchyLevel = 3, IsSystemRole = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = warehouseManagerId, Name = "WarehouseManager", Description = "Manage warehouses and stocks", HierarchyLevel = 4, IsSystemRole = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = purchaseManagerId, Name = "PurchaseManager", Description = "Manage purchase orders", HierarchyLevel = 5, IsSystemRole = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = salesManagerId, Name = "SalesManager", Description = "Manage sales orders", HierarchyLevel = 6, IsSystemRole = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = accountantId, Name = "Accountant", Description = "View financial data", HierarchyLevel = 7, IsSystemRole = false, CreatedAt = seedDate, UpdatedAt = seedDate },
            new Role { Id = viewerId, Name = "Viewer", Description = "Read-only access", HierarchyLevel = 10, IsSystemRole = false, CreatedAt = seedDate, UpdatedAt = seedDate }
        );

        var pUsersView = Guid.Parse("22222222-0000-0000-0000-000000000001");
        var pUsersCreate = Guid.Parse("22222222-0000-0000-0000-000000000002");
        var pUsersEdit = Guid.Parse("22222222-0000-0000-0000-000000000003");
        var pUsersDelete = Guid.Parse("22222222-0000-0000-0000-000000000004");
        var pUsersAssignRole = Guid.Parse("22222222-0000-0000-0000-000000000005");
        var pRolesView = Guid.Parse("22222222-0000-0000-0000-000000000006");
        var pCategoriesView = Guid.Parse("22222222-0000-0000-0000-000000000011");
        var pCategoriesCreate = Guid.Parse("22222222-0000-0000-0000-000000000012");
        var pCategoriesEdit = Guid.Parse("22222222-0000-0000-0000-000000000013");
        var pCategoriesDelete = Guid.Parse("22222222-0000-0000-0000-000000000014");
        var pProductsView = Guid.Parse("22222222-0000-0000-0000-000000000021");
        var pProductsCreate = Guid.Parse("22222222-0000-0000-0000-000000000022");
        var pProductsEdit = Guid.Parse("22222222-0000-0000-0000-000000000023");
        var pProductsDelete = Guid.Parse("22222222-0000-0000-0000-000000000024");
        var pWarehousesView = Guid.Parse("22222222-0000-0000-0000-000000000031");
        var pWarehousesCreate = Guid.Parse("22222222-0000-0000-0000-000000000032");
        var pWarehousesEdit = Guid.Parse("22222222-0000-0000-0000-000000000033");
        var pWarehousesDelete = Guid.Parse("22222222-0000-0000-0000-000000000034");
        var pStocksView = Guid.Parse("22222222-0000-0000-0000-000000000041");
        var pStocksAdjust = Guid.Parse("22222222-0000-0000-0000-000000000042");
        var pSuppliersView = Guid.Parse("22222222-0000-0000-0000-000000000051");
        var pSuppliersCreate = Guid.Parse("22222222-0000-0000-0000-000000000052");
        var pSuppliersEdit = Guid.Parse("22222222-0000-0000-0000-000000000053");
        var pSuppliersDelete = Guid.Parse("22222222-0000-0000-0000-000000000054");
        var pPOView = Guid.Parse("22222222-0000-0000-0000-000000000061");
        var pPOCreate = Guid.Parse("22222222-0000-0000-0000-000000000062");
        var pPOApprove = Guid.Parse("22222222-0000-0000-0000-000000000063");
        var pPOReceive = Guid.Parse("22222222-0000-0000-0000-000000000064");
        var pPOCancel = Guid.Parse("22222222-0000-0000-0000-000000000065");
        var pSOView = Guid.Parse("22222222-0000-0000-0000-000000000071");
        var pSOCreate = Guid.Parse("22222222-0000-0000-0000-000000000072");
        var pSOApprove = Guid.Parse("22222222-0000-0000-0000-000000000073");
        var pSOShip = Guid.Parse("22222222-0000-0000-0000-000000000074");
        var pSODeliver = Guid.Parse("22222222-0000-0000-0000-000000000075");
        var pSOCancel = Guid.Parse("22222222-0000-0000-0000-000000000076");

        builder.Entity<Permission>().HasData(
            new Permission { Id = pUsersView, Name = "Users.View", Module = "Users", Description = "View users", CreatedAt = seedDate },
            new Permission { Id = pUsersCreate, Name = "Users.Create", Module = "Users", Description = "Create users", CreatedAt = seedDate },
            new Permission { Id = pUsersEdit, Name = "Users.Edit", Module = "Users", Description = "Edit users", CreatedAt = seedDate },
            new Permission { Id = pUsersDelete, Name = "Users.Delete", Module = "Users", Description = "Delete users", CreatedAt = seedDate },
            new Permission { Id = pUsersAssignRole, Name = "Users.AssignRole", Module = "Users", Description = "Assign roles to users", CreatedAt = seedDate },
            new Permission { Id = pRolesView, Name = "Roles.View", Module = "Roles", Description = "View roles", CreatedAt = seedDate },
            new Permission { Id = pCategoriesView, Name = "Categories.View", Module = "Categories", Description = "View categories", CreatedAt = seedDate },
            new Permission { Id = pCategoriesCreate, Name = "Categories.Create", Module = "Categories", Description = "Create categories", CreatedAt = seedDate },
            new Permission { Id = pCategoriesEdit, Name = "Categories.Edit", Module = "Categories", Description = "Edit categories", CreatedAt = seedDate },
            new Permission { Id = pCategoriesDelete, Name = "Categories.Delete", Module = "Categories", Description = "Delete categories", CreatedAt = seedDate },
            new Permission { Id = pProductsView, Name = "Products.View", Module = "Products", Description = "View products", CreatedAt = seedDate },
            new Permission { Id = pProductsCreate, Name = "Products.Create", Module = "Products", Description = "Create products", CreatedAt = seedDate },
            new Permission { Id = pProductsEdit, Name = "Products.Edit", Module = "Products", Description = "Edit products", CreatedAt = seedDate },
            new Permission { Id = pProductsDelete, Name = "Products.Delete", Module = "Products", Description = "Delete products", CreatedAt = seedDate },
            new Permission { Id = pWarehousesView, Name = "Warehouses.View", Module = "Warehouses", Description = "View warehouses", CreatedAt = seedDate },
            new Permission { Id = pWarehousesCreate, Name = "Warehouses.Create", Module = "Warehouses", Description = "Create warehouses", CreatedAt = seedDate },
            new Permission { Id = pWarehousesEdit, Name = "Warehouses.Edit", Module = "Warehouses", Description = "Edit warehouses", CreatedAt = seedDate },
            new Permission { Id = pWarehousesDelete, Name = "Warehouses.Delete", Module = "Warehouses", Description = "Delete warehouses", CreatedAt = seedDate },
            new Permission { Id = pStocksView, Name = "Stocks.View", Module = "Stocks", Description = "View stock levels", CreatedAt = seedDate },
            new Permission { Id = pStocksAdjust, Name = "Stocks.Adjust", Module = "Stocks", Description = "Manually adjust stock", CreatedAt = seedDate },
            new Permission { Id = pSuppliersView, Name = "Suppliers.View", Module = "Suppliers", Description = "View suppliers", CreatedAt = seedDate },
            new Permission { Id = pSuppliersCreate, Name = "Suppliers.Create", Module = "Suppliers", Description = "Create suppliers", CreatedAt = seedDate },
            new Permission { Id = pSuppliersEdit, Name = "Suppliers.Edit", Module = "Suppliers", Description = "Edit suppliers", CreatedAt = seedDate },
            new Permission { Id = pSuppliersDelete, Name = "Suppliers.Delete", Module = "Suppliers", Description = "Delete suppliers", CreatedAt = seedDate },
            new Permission { Id = pPOView, Name = "PurchaseOrders.View", Module = "PurchaseOrders", Description = "View purchase orders", CreatedAt = seedDate },
            new Permission { Id = pPOCreate, Name = "PurchaseOrders.Create", Module = "PurchaseOrders", Description = "Create purchase orders", CreatedAt = seedDate },
            new Permission { Id = pPOApprove, Name = "PurchaseOrders.Approve", Module = "PurchaseOrders", Description = "Approve purchase orders", CreatedAt = seedDate },
            new Permission { Id = pPOReceive, Name = "PurchaseOrders.Receive", Module = "PurchaseOrders", Description = "Receive purchase orders", CreatedAt = seedDate },
            new Permission { Id = pPOCancel, Name = "PurchaseOrders.Cancel", Module = "PurchaseOrders", Description = "Cancel purchase orders", CreatedAt = seedDate },
            new Permission { Id = pSOView, Name = "SalesOrders.View", Module = "SalesOrders", Description = "View sales orders", CreatedAt = seedDate },
            new Permission { Id = pSOCreate, Name = "SalesOrders.Create", Module = "SalesOrders", Description = "Create sales orders", CreatedAt = seedDate },
            new Permission { Id = pSOApprove, Name = "SalesOrders.Approve", Module = "SalesOrders", Description = "Approve sales orders", CreatedAt = seedDate },
            new Permission { Id = pSOShip, Name = "SalesOrders.Ship", Module = "SalesOrders", Description = "Ship sales orders", CreatedAt = seedDate },
            new Permission { Id = pSODeliver, Name = "SalesOrders.Deliver", Module = "SalesOrders", Description = "Mark orders delivered", CreatedAt = seedDate },
            new Permission { Id = pSOCancel, Name = "SalesOrders.Cancel", Module = "SalesOrders", Description = "Cancel sales orders", CreatedAt = seedDate }
        );

        var allPermissions = new[] { pUsersView, pUsersCreate, pUsersEdit, pUsersDelete, pUsersAssignRole, pRolesView, pCategoriesView, pCategoriesCreate, pCategoriesEdit, pCategoriesDelete, pProductsView, pProductsCreate, pProductsEdit, pProductsDelete, pWarehousesView, pWarehousesCreate, pWarehousesEdit, pWarehousesDelete, pStocksView, pStocksAdjust, pSuppliersView, pSuppliersCreate, pSuppliersEdit, pSuppliersDelete, pPOView, pPOCreate, pPOApprove, pPOReceive, pPOCancel, pSOView, pSOCreate, pSOApprove, pSOShip, pSODeliver, pSOCancel };

        foreach (var permId in allPermissions)
        {
            builder.Entity<RolePermission>().HasData(new RolePermission { RoleId = superAdminId, PermissionId = permId });
            builder.Entity<RolePermission>().HasData(new RolePermission { RoleId = adminId, PermissionId = permId });
        }

        builder.Entity<RolePermission>().HasData(
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pCategoriesView },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pCategoriesCreate },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pCategoriesEdit },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pProductsView },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pProductsCreate },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pProductsEdit },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pProductsDelete },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pStocksView },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pStocksAdjust },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pWarehousesView },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pSuppliersView },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pPOView },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pSOView },
            new RolePermission { RoleId = inventoryManagerId, PermissionId = pRolesView },
            new RolePermission { RoleId = warehouseManagerId, PermissionId = pWarehousesView },
            new RolePermission { RoleId = warehouseManagerId, PermissionId = pWarehousesCreate },
            new RolePermission { RoleId = warehouseManagerId, PermissionId = pWarehousesEdit },
            new RolePermission { RoleId = warehouseManagerId, PermissionId = pStocksView },
            new RolePermission { RoleId = warehouseManagerId, PermissionId = pStocksAdjust },
            new RolePermission { RoleId = warehouseManagerId, PermissionId = pProductsView },
            new RolePermission { RoleId = warehouseManagerId, PermissionId = pPOView },
            new RolePermission { RoleId = warehouseManagerId, PermissionId = pPOReceive },
            new RolePermission { RoleId = warehouseManagerId, PermissionId = pSOView },
            new RolePermission { RoleId = warehouseManagerId, PermissionId = pRolesView },
            new RolePermission { RoleId = purchaseManagerId, PermissionId = pPOView },
            new RolePermission { RoleId = purchaseManagerId, PermissionId = pPOCreate },
            new RolePermission { RoleId = purchaseManagerId, PermissionId = pPOApprove },
            new RolePermission { RoleId = purchaseManagerId, PermissionId = pPOReceive },
            new RolePermission { RoleId = purchaseManagerId, PermissionId = pPOCancel },
            new RolePermission { RoleId = purchaseManagerId, PermissionId = pSuppliersView },
            new RolePermission { RoleId = purchaseManagerId, PermissionId = pSuppliersCreate },
            new RolePermission { RoleId = purchaseManagerId, PermissionId = pSuppliersEdit },
            new RolePermission { RoleId = purchaseManagerId, PermissionId = pProductsView },
            new RolePermission { RoleId = purchaseManagerId, PermissionId = pWarehousesView },
            new RolePermission { RoleId = purchaseManagerId, PermissionId = pStocksView },
            new RolePermission { RoleId = purchaseManagerId, PermissionId = pRolesView },
            new RolePermission { RoleId = salesManagerId, PermissionId = pSOView },
            new RolePermission { RoleId = salesManagerId, PermissionId = pSOCreate },
            new RolePermission { RoleId = salesManagerId, PermissionId = pSOApprove },
            new RolePermission { RoleId = salesManagerId, PermissionId = pSOShip },
            new RolePermission { RoleId = salesManagerId, PermissionId = pSODeliver },
            new RolePermission { RoleId = salesManagerId, PermissionId = pSOCancel },
            new RolePermission { RoleId = salesManagerId, PermissionId = pProductsView },
            new RolePermission { RoleId = salesManagerId, PermissionId = pStocksView },
            new RolePermission { RoleId = salesManagerId, PermissionId = pWarehousesView },
            new RolePermission { RoleId = salesManagerId, PermissionId = pRolesView },
            new RolePermission { RoleId = accountantId, PermissionId = pPOView },
            new RolePermission { RoleId = accountantId, PermissionId = pSOView },
            new RolePermission { RoleId = accountantId, PermissionId = pProductsView },
            new RolePermission { RoleId = accountantId, PermissionId = pStocksView },
            new RolePermission { RoleId = accountantId, PermissionId = pSuppliersView },
            new RolePermission { RoleId = accountantId, PermissionId = pWarehousesView },
            new RolePermission { RoleId = accountantId, PermissionId = pRolesView },
            new RolePermission { RoleId = viewerId, PermissionId = pUsersView },
            new RolePermission { RoleId = viewerId, PermissionId = pRolesView },
            new RolePermission { RoleId = viewerId, PermissionId = pCategoriesView },
            new RolePermission { RoleId = viewerId, PermissionId = pProductsView },
            new RolePermission { RoleId = viewerId, PermissionId = pWarehousesView },
            new RolePermission { RoleId = viewerId, PermissionId = pStocksView },
            new RolePermission { RoleId = viewerId, PermissionId = pSuppliersView },
            new RolePermission { RoleId = viewerId, PermissionId = pPOView },
            new RolePermission { RoleId = viewerId, PermissionId = pSOView }
        );
    }
}