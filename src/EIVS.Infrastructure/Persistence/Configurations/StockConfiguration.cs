using EcommerceInventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventorySystem.Infrastructure.Persistence.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Quantity).IsRequired();
        builder.Property(s => s.LowStockThreshold).HasDefaultValue(10);

        builder.HasOne(s => s.Product)
               .WithOne(p => p.Stock)
               .HasForeignKey<Stock>(s => s.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}