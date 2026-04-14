using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EcommerceInventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CloudinaryId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_categories_categories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    HierarchyLevel = table.Column<int>(type: "integer", nullable: false),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_street = table.Column<string>(type: "text", nullable: true),
                    address_city = table.Column<string>(type: "text", nullable: true),
                    address_state = table.Column<string>(type: "text", nullable: true),
                    address_pincode = table.Column<string>(type: "text", nullable: true),
                    address_country = table.Column<string>(type: "text", nullable: true),
                    GstNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CloudinaryProfileId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReorderLevel = table.Column<int>(type: "integer", nullable: false),
                    ReorderQty = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(10,3)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "email_verification_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_verification_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_email_verification_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_password_reset_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedBy = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeviceInfo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "warehouses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    address_street = table.Column<string>(type: "text", nullable: true),
                    address_city = table.Column<string>(type: "text", nullable: true),
                    address_state = table.Column<string>(type: "text", nullable: true),
                    address_pincode = table.Column<string>(type: "text", nullable: true),
                    address_country = table.Column<string>(type: "text", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_warehouses_users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "product_images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CloudinaryId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_images_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PoNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ExpectedDeliveryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_orders_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_orders_warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SoNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: "Walk-in Customer"),
                    CustomerEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CustomerPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ShippingAddressJson = table.Column<string>(type: "jsonb", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_orders_warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ReservedQty = table.Column<int>(type: "integer", nullable: false),
                    LastCountedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stocks_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stocks_warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityOrdered = table.Column<int>(type: "integer", nullable: false),
                    QuantityReceived = table.Column<int>(type: "integer", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_order_items_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_order_items_purchase_orders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sales_order_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_order_items_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sales_order_items_sales_orders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "sales_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_movements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    QuantityBefore = table.Column<int>(type: "integer", nullable: false),
                    QuantityAfter = table.Column<int>(type: "integer", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    PerformedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_movements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_movements_stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "CreatedAt", "Description", "Module", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("21111111-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View users", "Users", "Users.View", new DateTime(2026, 4, 14, 12, 11, 54, 151, DateTimeKind.Utc).AddTicks(8953) },
                    { new Guid("21111111-0000-0000-0000-000000000002"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create users", "Users", "Users.Create", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(340) },
                    { new Guid("21111111-0000-0000-0000-000000000003"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit users", "Users", "Users.Edit", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(342) },
                    { new Guid("21111111-0000-0000-0000-000000000004"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete users", "Users", "Users.Delete", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(367) },
                    { new Guid("21111111-0000-0000-0000-000000000005"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Assign/revoke roles", "Users", "Users.AssignRole", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(369) },
                    { new Guid("21111111-0000-0000-0000-000000000010"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View roles", "Roles", "Roles.View", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(370) },
                    { new Guid("21111111-0000-0000-0000-000000000020"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View categories", "Categories", "Categories.View", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(372) },
                    { new Guid("21111111-0000-0000-0000-000000000021"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create categories", "Categories", "Categories.Create", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(374) },
                    { new Guid("21111111-0000-0000-0000-000000000022"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit categories", "Categories", "Categories.Edit", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(375) },
                    { new Guid("21111111-0000-0000-0000-000000000023"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete categories", "Categories", "Categories.Delete", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(377) },
                    { new Guid("21111111-0000-0000-0000-000000000030"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View products", "Products", "Products.View", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(379) },
                    { new Guid("21111111-0000-0000-0000-000000000031"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create products", "Products", "Products.Create", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(380) },
                    { new Guid("21111111-0000-0000-0000-000000000032"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit products", "Products", "Products.Edit", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(382) },
                    { new Guid("21111111-0000-0000-0000-000000000033"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete products", "Products", "Products.Delete", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(384) },
                    { new Guid("21111111-0000-0000-0000-000000000040"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View warehouses", "Warehouses", "Warehouses.View", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(385) },
                    { new Guid("21111111-0000-0000-0000-000000000041"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create warehouses", "Warehouses", "Warehouses.Create", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(387) },
                    { new Guid("21111111-0000-0000-0000-000000000042"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit warehouses", "Warehouses", "Warehouses.Edit", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(389) },
                    { new Guid("21111111-0000-0000-0000-000000000043"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete warehouses", "Warehouses", "Warehouses.Delete", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(390) },
                    { new Guid("21111111-0000-0000-0000-000000000050"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View stock levels", "Stocks", "Stocks.View", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(392) },
                    { new Guid("21111111-0000-0000-0000-000000000051"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Adjust stock manually", "Stocks", "Stocks.Adjust", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(393) },
                    { new Guid("21111111-0000-0000-0000-000000000060"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View suppliers", "Suppliers", "Suppliers.View", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(395) },
                    { new Guid("21111111-0000-0000-0000-000000000061"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create suppliers", "Suppliers", "Suppliers.Create", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(396) },
                    { new Guid("21111111-0000-0000-0000-000000000062"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Edit suppliers", "Suppliers", "Suppliers.Edit", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(420) },
                    { new Guid("21111111-0000-0000-0000-000000000063"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Delete suppliers", "Suppliers", "Suppliers.Delete", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(422) },
                    { new Guid("21111111-0000-0000-0000-000000000070"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View purchase orders", "PurchaseOrders", "PurchaseOrders.View", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(424) },
                    { new Guid("21111111-0000-0000-0000-000000000071"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create/manage purchase orders", "PurchaseOrders", "PurchaseOrders.Create", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(426) },
                    { new Guid("21111111-0000-0000-0000-000000000072"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Approve/reject purchase orders", "PurchaseOrders", "PurchaseOrders.Approve", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(427) },
                    { new Guid("21111111-0000-0000-0000-000000000073"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Receive purchase orders", "PurchaseOrders", "PurchaseOrders.Receive", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(429) },
                    { new Guid("21111111-0000-0000-0000-000000000074"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cancel purchase orders", "PurchaseOrders", "PurchaseOrders.Cancel", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(431) },
                    { new Guid("21111111-0000-0000-0000-000000000080"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View sales orders", "SalesOrders", "SalesOrders.View", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(432) },
                    { new Guid("21111111-0000-0000-0000-000000000081"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create/manage sales orders", "SalesOrders", "SalesOrders.Create", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(434) },
                    { new Guid("21111111-0000-0000-0000-000000000082"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Approve sales orders", "SalesOrders", "SalesOrders.Approve", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(435) },
                    { new Guid("21111111-0000-0000-0000-000000000083"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ship sales orders", "SalesOrders", "SalesOrders.Ship", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(437) },
                    { new Guid("21111111-0000-0000-0000-000000000084"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mark sales orders delivered", "SalesOrders", "SalesOrders.Deliver", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(439) },
                    { new Guid("21111111-0000-0000-0000-000000000085"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cancel sales orders", "SalesOrders", "SalesOrders.Cancel", new DateTime(2026, 4, 14, 12, 11, 54, 152, DateTimeKind.Utc).AddTicks(440) }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "CreatedAt", "Description", "HierarchyLevel", "IsSystemRole", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, "SuperAdmin", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000002"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, "Admin", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000003"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, false, "InventoryManager", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000004"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, false, "WarehouseManager", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000005"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, false, "PurchaseManager", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000006"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, false, "SalesManager", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000007"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, false, "Accountant", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-0000-0000-0000-000000000008"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 10, false, "Viewer", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("21111111-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000002"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000003"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000004"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000005"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000010"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000020"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000022"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000023"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000030"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000032"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000033"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000040"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000042"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000043"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000050"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000060"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000062"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000063"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000070"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000072"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000073"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000074"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000080"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000081"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000082"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000083"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000084"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000085"), new Guid("11111111-0000-0000-0000-000000000001") },
                    { new Guid("21111111-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000002"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000003"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000005"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000010"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000020"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000022"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000023"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000030"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000032"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000033"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000040"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000041"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000042"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000043"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000050"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000060"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000062"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000063"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000070"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000072"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000073"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000074"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000080"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000081"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000082"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000083"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000084"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000085"), new Guid("11111111-0000-0000-0000-000000000002") },
                    { new Guid("21111111-0000-0000-0000-000000000020"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("21111111-0000-0000-0000-000000000021"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("21111111-0000-0000-0000-000000000022"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("21111111-0000-0000-0000-000000000030"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("21111111-0000-0000-0000-000000000031"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("21111111-0000-0000-0000-000000000032"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("21111111-0000-0000-0000-000000000040"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("21111111-0000-0000-0000-000000000050"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("21111111-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000003") },
                    { new Guid("21111111-0000-0000-0000-000000000040"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("21111111-0000-0000-0000-000000000050"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("21111111-0000-0000-0000-000000000051"), new Guid("11111111-0000-0000-0000-000000000004") },
                    { new Guid("21111111-0000-0000-0000-000000000030"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("21111111-0000-0000-0000-000000000040"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("21111111-0000-0000-0000-000000000060"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("21111111-0000-0000-0000-000000000061"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("21111111-0000-0000-0000-000000000062"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("21111111-0000-0000-0000-000000000070"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("21111111-0000-0000-0000-000000000071"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("21111111-0000-0000-0000-000000000072"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("21111111-0000-0000-0000-000000000073"), new Guid("11111111-0000-0000-0000-000000000005") },
                    { new Guid("21111111-0000-0000-0000-000000000030"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("21111111-0000-0000-0000-000000000040"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("21111111-0000-0000-0000-000000000050"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("21111111-0000-0000-0000-000000000080"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("21111111-0000-0000-0000-000000000081"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("21111111-0000-0000-0000-000000000082"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("21111111-0000-0000-0000-000000000083"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("21111111-0000-0000-0000-000000000084"), new Guid("11111111-0000-0000-0000-000000000006") },
                    { new Guid("21111111-0000-0000-0000-000000000030"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("21111111-0000-0000-0000-000000000040"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("21111111-0000-0000-0000-000000000050"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("21111111-0000-0000-0000-000000000060"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("21111111-0000-0000-0000-000000000070"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("21111111-0000-0000-0000-000000000080"), new Guid("11111111-0000-0000-0000-000000000007") },
                    { new Guid("21111111-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("21111111-0000-0000-0000-000000000010"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("21111111-0000-0000-0000-000000000020"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("21111111-0000-0000-0000-000000000030"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("21111111-0000-0000-0000-000000000040"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("21111111-0000-0000-0000-000000000050"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("21111111-0000-0000-0000-000000000060"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("21111111-0000-0000-0000-000000000070"), new Guid("11111111-0000-0000-0000-000000000008") },
                    { new Guid("21111111-0000-0000-0000-000000000080"), new Guid("11111111-0000-0000-0000-000000000008") }
                });

            migrationBuilder.CreateIndex(
                name: "idx_categories_parent",
                table: "categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "idx_categories_slug",
                table: "categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_email_verification_tokens_UserId",
                table: "email_verification_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_UserId",
                table: "password_reset_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_Name",
                table: "permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_product_images_product",
                table: "product_images",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "idx_products_category",
                table: "products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "idx_products_status",
                table: "products",
                column: "Status",
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_products_Sku",
                table: "products",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_Slug",
                table: "products",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_items_ProductId",
                table: "purchase_order_items",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_items_PurchaseOrderId",
                table: "purchase_order_items",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "idx_po_status",
                table: "purchase_orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_po_supplier",
                table: "purchase_orders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_PoNumber",
                table: "purchase_orders",
                column: "PoNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_WarehouseId",
                table: "purchase_orders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                table: "refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_PermissionId",
                table: "role_permissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Name",
                table: "roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sales_order_items_ProductId",
                table: "sales_order_items",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_order_items_SalesOrderId",
                table: "sales_order_items",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "idx_so_status",
                table: "sales_orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_sales_orders_SoNumber",
                table: "sales_orders",
                column: "SoNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sales_orders_WarehouseId",
                table: "sales_orders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "idx_stock_movements_ref",
                table: "stock_movements",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "idx_stock_movements_stock",
                table: "stock_movements",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "idx_stocks_product",
                table: "stocks",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "idx_stocks_warehouse",
                table: "stocks",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_stocks_ProductId_WarehouseId",
                table: "stocks",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                table: "user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "idx_users_status",
                table: "users",
                column: "Status",
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_Code",
                table: "warehouses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_ManagerId",
                table: "warehouses",
                column: "ManagerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_verification_tokens");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "product_images");

            migrationBuilder.DropTable(
                name: "purchase_order_items");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "sales_order_items");

            migrationBuilder.DropTable(
                name: "stock_movements");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "purchase_orders");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "sales_orders");

            migrationBuilder.DropTable(
                name: "stocks");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "warehouses");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
