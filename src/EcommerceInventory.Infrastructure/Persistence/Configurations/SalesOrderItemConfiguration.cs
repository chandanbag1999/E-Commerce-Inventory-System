using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        builder.ToTable("sales_order_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(i => i.SalesOrderId).HasColumnName("sales_order_id").IsRequired();
        builder.Property(i => i.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(i => i.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(i => i.UnitPrice).HasColumnName("unit_price").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(i => i.Discount).HasColumnName("discount").HasColumnType("numeric(18,2)").HasDefaultValue(0m).IsRequired();
        builder.Ignore(i => i.LineTotal);
        builder.Ignore(i => i.UpdatedAt);
        builder.Ignore(i => i.CreatedAt);

        builder.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}