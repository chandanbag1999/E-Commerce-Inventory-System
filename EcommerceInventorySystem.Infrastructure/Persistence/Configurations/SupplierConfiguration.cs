using EcommerceInventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventorySystem.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.ContactEmail).IsRequired().HasMaxLength(150);
        builder.HasIndex(s => s.ContactEmail).IsUnique();
        builder.Property(s => s.Phone).HasMaxLength(20);
        builder.Property(s => s.Address).HasMaxLength(300);
    }
}