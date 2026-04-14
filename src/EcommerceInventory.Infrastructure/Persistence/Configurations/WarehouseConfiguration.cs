using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(w => w.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(w => w.Code)
            .IsUnique();

        builder.Property(w => w.Phone)
            .HasMaxLength(20);

        builder.Property(w => w.IsActive)
            .IsRequired();

        builder.Property(w => w.DeletedAt);

        // Address stored as JSONB
        builder.OwnsOne(w => w.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("address_street");
            address.Property(a => a.City).HasColumnName("address_city");
            address.Property(a => a.State).HasColumnName("address_state");
            address.Property(a => a.Pincode).HasColumnName("address_pincode");
            address.Property(a => a.Country).HasColumnName("address_country");
        });

        builder.HasOne(w => w.Manager)
            .WithMany()
            .HasForeignKey(w => w.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
