using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("purchase_order_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(i => i.PurchaseOrderId).HasColumnName("purchase_order_id").IsRequired();
        builder.Property(i => i.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(i => i.QuantityOrdered).HasColumnName("quantity_ordered").IsRequired();
        builder.Property(i => i.QuantityReceived).HasColumnName("quantity_received").HasDefaultValue(0).IsRequired();
        builder.Property(i => i.UnitCost).HasColumnName("unit_cost").HasColumnType("numeric(18,2)").IsRequired();
        builder.Ignore(i => i.TotalCost);
        builder.Ignore(i => i.UpdatedAt);
        builder.Ignore(i => i.CreatedAt);

        builder.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}