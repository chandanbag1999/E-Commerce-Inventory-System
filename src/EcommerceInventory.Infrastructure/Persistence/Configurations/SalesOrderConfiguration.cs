using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("sales_orders");
        builder.HasKey(so => so.Id);
        builder.Property(so => so.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(so => so.SoNumber).HasColumnName("so_number").HasMaxLength(50).IsRequired();
        builder.HasIndex(so => so.SoNumber).IsUnique().HasDatabaseName("idx_so_number");
        builder.Property(so => so.CustomerName).HasColumnName("customer_name").HasMaxLength(200).HasDefaultValue("Walk-in Customer").IsRequired();
        builder.Property(so => so.CustomerEmail).HasColumnName("customer_email").HasMaxLength(200);
        builder.Property(so => so.CustomerPhone).HasColumnName("customer_phone").HasMaxLength(20);
        builder.Property(so => so.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(so => so.Status).HasColumnName("status").HasMaxLength(30).HasConversion<string>().HasDefaultValue(OrderStatus.Draft).IsRequired();
        builder.Property(so => so.Subtotal).HasColumnName("subtotal").HasColumnType("numeric(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(so => so.TotalAmount).HasColumnName("total_amount").HasColumnType("numeric(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(so => so.Notes).HasColumnName("notes").HasColumnType("text");
        builder.Property(so => so.ShippingAddress).HasColumnName("shipping_address_json").HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, _jsonOpts),
                v => v == null ? null : JsonSerializer.Deserialize<Address>(v, _jsonOpts));
        builder.Property(so => so.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(so => so.ApprovedBy).HasColumnName("approved_by");
        builder.Property(so => so.ApprovedAt).HasColumnName("approved_at");
        builder.Property(so => so.ShippedAt).HasColumnName("shipped_at");
        builder.Property(so => so.DeliveredAt).HasColumnName("delivered_at");
        builder.Property(so => so.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(so => so.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(so => so.Status).HasDatabaseName("idx_so_status");

        builder.HasOne(so => so.Warehouse).WithMany().HasForeignKey(so => so.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(so => so.Items).WithOne(i => i.SalesOrder).HasForeignKey(i => i.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
