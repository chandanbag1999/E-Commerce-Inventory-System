using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.ContactName)
            .HasMaxLength(150);

        builder.Property(s => s.Email)
            .HasMaxLength(200);

        builder.Property(s => s.Phone)
            .HasMaxLength(20);

        builder.Property(s => s.GstNumber)
            .HasMaxLength(50);

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.Property(s => s.DeletedAt);

        // Address stored as JSONB
        builder.OwnsOne(s => s.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("address_street");
            address.Property(a => a.City).HasColumnName("address_city");
            address.Property(a => a.State).HasColumnName("address_state");
            address.Property(a => a.Pincode).HasColumnName("address_pincode");
            address.Property(a => a.Country).HasColumnName("address_country");
        });
    }
}
