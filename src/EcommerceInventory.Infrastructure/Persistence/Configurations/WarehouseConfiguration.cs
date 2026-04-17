using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(w => w.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(w => w.Code).HasColumnName("code").HasMaxLength(20).IsRequired();
        builder.HasIndex(w => w.Code).IsUnique().HasDatabaseName("idx_warehouses_code");
        builder.Property(w => w.Address).HasColumnName("address_json").HasColumnType("jsonb")
            .HasConversion(v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                         v => v == null ? null : JsonSerializer.Deserialize<Address>(v, (JsonSerializerOptions?)null));
        builder.Property(w => w.ManagerId).HasColumnName("manager_id");
        builder.Property(w => w.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(w => w.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        builder.Property(w => w.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(w => w.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(w => w.DeletedAt).HasColumnName("deleted_at");
        builder.HasQueryFilter(w => w.DeletedAt == null);

        builder.HasOne(w => w.Manager).WithMany().HasForeignKey(w => w.ManagerId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(w => w.Stocks).WithOne(s => s.Warehouse).HasForeignKey(s => s.WarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}