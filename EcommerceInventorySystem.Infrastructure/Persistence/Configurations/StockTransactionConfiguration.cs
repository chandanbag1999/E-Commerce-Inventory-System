using EcommerceInventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventorySystem.Infrastructure.Persistence.Configurations;

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.HasKey(st => st.Id);
        builder.Property(st => st.Quantity).IsRequired();
        builder.Property(st => st.TransactionType).HasConversion<string>();
        builder.Property(st => st.Note).HasMaxLength(500);
        builder.Property(st => st.CreatedBy).IsRequired().HasMaxLength(150);

        builder.HasOne(st => st.Product)
               .WithMany()
               .HasForeignKey(st => st.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}