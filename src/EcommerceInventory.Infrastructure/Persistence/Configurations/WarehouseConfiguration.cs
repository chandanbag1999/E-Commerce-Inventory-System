using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(w => w.Name)
            .HasColumnName("name").HasMaxLength(150).IsRequired();

        builder.Property(w => w.Code)
            .HasColumnName("code").HasMaxLength(20).IsRequired();
        builder.HasIndex(w => w.Code)
            .IsUnique().HasDatabaseName("idx_warehouses_code");

        builder.Property(w => w.Address)
            .HasColumnName("address_json").HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, _jsonOpts),
                v => v == null ? null : JsonSerializer.Deserialize<Address>(v, _jsonOpts));

        builder.Property(w => w.ManagerId).HasColumnName("manager_id");
        builder.Property(w => w.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(w => w.Email).HasColumnName("email").HasMaxLength(200);
        builder.Property(w => w.Capacity).HasColumnName("capacity");

        builder.Property(w => w.IsActive)
            .HasColumnName("is_active").HasDefaultValue(true).IsRequired();

        builder.Property(w => w.Version)
            .HasColumnName("version").IsConcurrencyToken().HasDefaultValue(0);

        builder.Property(w => w.CreatedBy).HasColumnName("created_by");
        builder.Property(w => w.UpdatedBy).HasColumnName("updated_by");

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(w => w.DeletedAt).HasColumnName("deleted_at");

        // #12: missing indexes
        builder.HasIndex(w => w.IsActive).HasDatabaseName("idx_warehouses_active");
        builder.HasIndex(w => w.ManagerId).HasDatabaseName("idx_warehouses_manager");

        builder.HasQueryFilter(w => w.DeletedAt == null);

        builder.HasOne(w => w.Manager)
            .WithMany().HasForeignKey(w => w.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(w => w.Stocks)
            .WithOne(s => s.Warehouse).HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
