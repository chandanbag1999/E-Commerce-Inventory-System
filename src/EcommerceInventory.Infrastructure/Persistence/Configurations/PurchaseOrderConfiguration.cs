using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.PoNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(po => po.PoNumber)
            .IsUnique();

        builder.Property(po => po.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(po => po.TotalAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(po => po.Notes)
            .HasColumnType("text");

        builder.HasOne(po => po.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(po => po.Warehouse)
            .WithMany()
            .HasForeignKey(po => po.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(po => po.Status)
            .HasDatabaseName("idx_po_status");

        builder.HasIndex(po => po.SupplierId)
            .HasDatabaseName("idx_po_supplier");
    }
}
