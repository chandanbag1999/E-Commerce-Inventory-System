using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WarehouseEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_warehouses_manager_id",
                table: "warehouses",
                newName: "idx_warehouses_manager");

            migrationBuilder.AddColumn<int>(
                name: "capacity",
                table: "warehouses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "warehouses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "warehouses",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "warehouses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "warehouses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "idx_warehouses_active",
                table: "warehouses",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_warehouses_active",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "capacity",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "email",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "version",
                table: "warehouses");

            migrationBuilder.RenameIndex(
                name: "idx_warehouses_manager",
                table: "warehouses",
                newName: "IX_warehouses_manager_id");
        }
    }
}
