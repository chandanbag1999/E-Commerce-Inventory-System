using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        builder.ToTable("sales_order_items");

        builder.HasKey(soi => soi.Id);

        builder.Property(soi => soi.Quantity)
            .IsRequired();

        builder.Property(soi => soi.UnitPrice)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(soi => soi.Discount)
            .HasColumnType("numeric(18,2)")
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(soi => soi.SalesOrder)
            .WithMany(so => so.Items)
            .HasForeignKey(soi => soi.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(soi => soi.Product)
            .WithMany(p => p.SalesOrderItems)
            .HasForeignKey(soi => soi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
