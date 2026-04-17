using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EcommerceInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "created_at", "description", "module", "name" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View users", "Users", "Users.View" },
                    { new Guid("22222222-0000-0000-0000-000000000002"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create users", "Users", "Users.Create" },
                    { new Guid("22222222-0000-0000-0000-000000000003"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit users", "Users", "Users.Edit" },
                    { new Guid("22222222-0000-0000-0000-000000000004"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete users", "Users", "Users.Delete" },
                    { new Guid("22222222-0000-0000-0000-000000000005"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Assign roles to users", "Users", "Users.AssignRole" },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View roles", "Roles", "Roles.View" },
                    { new Guid("22222222-0000-0000-0000-000000000011"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View categories", "Categories", "Categories.View" },
                    { new Guid("22222222-0000-0000-0000-000000000012"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create categories", "Categories", "Categories.Create" },
                    { new Guid("22222222-0000-0000-0000-000000000013"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit categories", "Categories", "Categories.Edit" },
                    { new Guid("22222222-0000-0000-0000-000000000014"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete categories", "Categories", "Categories.Delete" },
                    { new Guid("22222222-0000-0000-0000-000000000021"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View products", "Products", "Products.View" },
                    { new Guid("22222222-0000-0000-0000-000000000022"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create products", "Products", "Products.Create" },
                    { new Guid("22222222-0000-0000-0000-000000000023"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit products", "Products", "Products.Edit" },
                    { new Guid("22222222-0000-0000-0000-000000000024"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete products", "Products", "Products.Delete" },
                    { new Guid("22222222-0000-0000-0000-000000000031"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View warehouses", "Warehouses", "Warehouses.View" },
                    { new Guid("22222222-0000-0000-0000-000000000032"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create warehouses", "Warehouses", "Warehouses.Create" },
                    { new Guid("22222222-0000-0000-0000-000000000033"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit warehouses", "Warehouses", "Warehouses.Edit" },
                    { new Guid("22222222-0000-0000-0000-000000000034"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete warehouses", "Warehouses", "Warehouses.Delete" },
                    { new Guid("22222222-0000-0000-0000-000000000041"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View stock levels", "Stocks", "Stocks.View" },
                    { new Guid("22222222-0000-0000-0000-000000000042"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manually adjust stock", "Stocks", "Stocks.Adjust" },
                    { new Guid("22222222-0000-0000-0000-000000000051"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View suppliers", "Suppliers", "Suppliers.View" },
                    { new Guid("22222222-0000-0000-0000-000000000052"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create suppliers", "Suppliers", "Suppliers.Create" },
                    { new Guid("22222222-0000-0000-0000-000000000053"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit suppliers", "Suppliers", "Suppliers.Edit" },
                    { new Guid("22222222-0000-0000-0000-000000000054"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete suppliers", "Suppliers", "Suppliers.Delete" },
                    { new Guid("22222222-0000-0000-0000-000000000061"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View purchase orders", "PurchaseOrders", "PurchaseOrders.View" },
                    { new Guid("22222222-0000-0000-0000-000000000062"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create purchase orders", "PurchaseOrders", "PurchaseOrders.Create" },
                    { new Guid("22222222-0000-0000-0000-000000000063"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Approve purchase orders", "PurchaseOrders", "PurchaseOrders.Approve" },
                    { new Guid("22222222-0000-0000-0000-000000000064"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Receive purchase orders", "PurchaseOrders", "PurchaseOrders.Receive" },
                    { new Guid("22222222-0000-0000-0000-000000000065"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cancel purchase orders", "PurchaseOrders", "PurchaseOrders.Cancel" },
                    { new Guid("22222222-0000-0000-0000-000000000071"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View sales orders", "SalesOrders", "SalesOrders.View" },
                    { new Guid("22222222-0000-0000-0000-000000000072"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create sales orders", "SalesOrders", "SalesOrders.Create" },
                    { new Guid("22222222-0000-0000-0000-000000000073"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Approve sales orders", "SalesOrders", "SalesOrders.Approve" },
                    { new Guid("22222222-0000-0000-0000-000000000074"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ship sales orders", "SalesOrders", "SalesOrders.Ship" },
                    { new Guid("22222222-0000-0000-0000-000000000075"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mark orders delivered", "SalesOrders", "SalesOrders.Deliver" },
                    { new Guid("22222222-0000-0000-0000-000000000076"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cancel sales orders", "SalesOrders", "SalesOrders.Cancel" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "created_at", "description", "hierarchy_level", "is_system_role", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Full system access", 1, true, "SuperAdmin", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000002"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrative access", 2, true, "Admin", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "created_at", "description", "hierarchy_level", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000003"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manage inventory and products", 3, "InventoryManager", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000004"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manage warehouses and stocks", 4, "WarehouseManager", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000005"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manage purchase orders", 5, "PurchaseManager", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000006"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manage sales orders", 6, "SalesManager", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000007"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View financial data", 7, "Accountant", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000008"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read-only access", 10, "Viewer", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000002"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000003"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000004"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000005"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000012"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000013"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000014"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000022"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000023"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000024"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000032"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000033"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000034"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000042"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000052"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000053"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000054"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000062"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000063"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000064"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000065"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000072"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000073"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000074"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000075"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000076"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("22222222-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000002"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000003"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000004"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000005"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000012"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000013"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000014"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000022"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000023"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000024"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000032"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000033"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000034"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000042"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000052"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000053"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000054"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000062"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000063"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000064"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000065"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000072"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000073"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000074"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000075"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000076"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000012"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000013"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000022"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000023"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000024"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000042"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("22222222-0000-0000-0000-000000000032"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("22222222-0000-0000-0000-000000000033"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("22222222-0000-0000-0000-000000000042"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("22222222-0000-0000-0000-000000000064"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("22222222-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("22222222-0000-0000-0000-000000000052"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("22222222-0000-0000-0000-000000000053"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("22222222-0000-0000-0000-000000000062"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("22222222-0000-0000-0000-000000000063"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("22222222-0000-0000-0000-000000000064"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("22222222-0000-0000-0000-000000000065"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("22222222-0000-0000-0000-000000000072"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("22222222-0000-0000-0000-000000000073"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("22222222-0000-0000-0000-000000000074"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("22222222-0000-0000-0000-000000000075"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("22222222-0000-0000-0000-000000000076"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("22222222-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("22222222-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("22222222-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000008") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000002"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000003"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000004"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000005"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000012"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000013"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000014"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000022"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000023"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000024"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000032"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000033"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000034"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000042"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000052"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000053"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000054"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000062"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000063"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000064"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000065"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000072"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000073"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000074"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000075"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000076"), new Guid("11111111-0000-0000-0000-000000000001") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000002"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000003"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000004"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000005"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000012"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000013"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000014"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000022"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000023"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000024"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000032"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000033"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000034"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000042"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000052"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000053"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000054"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000062"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000063"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000064"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000065"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000072"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000073"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000074"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000075"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000076"), new Guid("11111111-0000-0000-0000-000000000002") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000012"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000013"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000022"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000023"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000024"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000042"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000003") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000004") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000004") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000004") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000032"), new Guid("11111111-0000-0000-0000-000000000004") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000033"), new Guid("11111111-0000-0000-0000-000000000004") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000004") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000042"), new Guid("11111111-0000-0000-0000-000000000004") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000004") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000064"), new Guid("11111111-0000-0000-0000-000000000004") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000004") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000005") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000005") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000005") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000005") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000005") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000052"), new Guid("11111111-0000-0000-0000-000000000005") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000053"), new Guid("11111111-0000-0000-0000-000000000005") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000005") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000062"), new Guid("11111111-0000-0000-0000-000000000005") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000063"), new Guid("11111111-0000-0000-0000-000000000005") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000064"), new Guid("11111111-0000-0000-0000-000000000005") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000065"), new Guid("11111111-0000-0000-0000-000000000005") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000006") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000006") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000006") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000006") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000006") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000072"), new Guid("11111111-0000-0000-0000-000000000006") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000073"), new Guid("11111111-0000-0000-0000-000000000006") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000074"), new Guid("11111111-0000-0000-0000-000000000006") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000075"), new Guid("11111111-0000-0000-0000-000000000006") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000076"), new Guid("11111111-0000-0000-0000-000000000006") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000007") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000007") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000007") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000007") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000007") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000007") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000007") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000008") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000008") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000011"), new Guid("11111111-0000-0000-0000-000000000008") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000008") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000008") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000008") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000008") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000008") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("22222222-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000008") });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000031"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000032"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000033"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000034"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000041"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000042"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000051"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000052"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000053"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000054"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000061"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000062"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000063"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000064"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000065"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000071"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000072"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000073"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000074"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000075"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000076"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("11111111-0000-0000-0000-000000000008"));
        }
    }
}
