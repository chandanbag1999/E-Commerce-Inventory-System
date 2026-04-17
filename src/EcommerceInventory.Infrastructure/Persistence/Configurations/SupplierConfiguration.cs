using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(s => s.ContactName).HasColumnName("contact_name").HasMaxLength(150);
        builder.Property(s => s.Email).HasColumnName("email").HasMaxLength(200);
        builder.Property(s => s.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(s => s.Address).HasColumnName("address_json").HasColumnType("jsonb")
            .HasConversion(v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                         v => v == null ? null : JsonSerializer.Deserialize<Address>(v, (JsonSerializerOptions?)null));
        builder.Property(s => s.GstNumber).HasColumnName("gst_number").HasMaxLength(50);
        builder.Property(s => s.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(s => s.DeletedAt).HasColumnName("deleted_at");
        builder.HasQueryFilter(s => s.DeletedAt == null);

        builder.HasMany(s => s.PurchaseOrders).WithOne(po => po.Supplier).HasForeignKey(po => po.SupplierId).OnDelete(DeleteBehavior.Restrict);
    }
}