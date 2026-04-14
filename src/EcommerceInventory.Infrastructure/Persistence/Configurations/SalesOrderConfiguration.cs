using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("sales_orders");

        builder.HasKey(so => so.Id);

        builder.Property(so => so.SoNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(so => so.SoNumber)
            .IsUnique();

        builder.Property(so => so.CustomerName)
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue("Walk-in Customer");

        builder.Property(so => so.CustomerEmail)
            .HasMaxLength(200);

        builder.Property(so => so.CustomerPhone)
            .HasMaxLength(20);

        builder.Property(so => so.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(so => so.Subtotal)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(so => so.TotalAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(so => so.Notes)
            .HasColumnType("text");

        builder.Property(so => so.ShippingAddressJson)
            .HasColumnType("jsonb");

        builder.HasOne(so => so.Warehouse)
            .WithMany()
            .HasForeignKey(so => so.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(so => so.Status)
            .HasDatabaseName("idx_so_status");
    }
}
