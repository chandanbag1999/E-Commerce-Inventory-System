using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders");
        builder.HasKey(po => po.Id);
        builder.Property(po => po.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(po => po.PoNumber).HasColumnName("po_number").HasMaxLength(50).IsRequired();
        builder.HasIndex(po => po.PoNumber).IsUnique().HasDatabaseName("idx_po_number");
        builder.Property(po => po.SupplierId).HasColumnName("supplier_id").IsRequired();
        builder.Property(po => po.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(po => po.Status).HasColumnName("status").HasMaxLength(30).HasConversion<string>().HasDefaultValue(OrderStatus.Draft).IsRequired();
        builder.Property(po => po.TotalAmount).HasColumnName("total_amount").HasColumnType("numeric(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(po => po.Notes).HasColumnName("notes").HasColumnType("text");
        builder.Property(po => po.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(po => po.ApprovedBy).HasColumnName("approved_by");
        builder.Property(po => po.ApprovedAt).HasColumnName("approved_at");
        builder.Property(po => po.ExpectedDeliveryAt).HasColumnName("expected_delivery_at");
        builder.Property(po => po.ReceivedAt).HasColumnName("received_at");
        builder.Property(po => po.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(po => po.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(po => po.Status).HasDatabaseName("idx_po_status");
        builder.HasIndex(po => po.SupplierId).HasDatabaseName("idx_po_supplier");

        builder.HasOne(po => po.Supplier).WithMany(s => s.PurchaseOrders).HasForeignKey(po => po.SupplierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(po => po.Warehouse).WithMany().HasForeignKey(po => po.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(po => po.Items).WithOne(i => i.PurchaseOrder).HasForeignKey(i => i.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
    }
}