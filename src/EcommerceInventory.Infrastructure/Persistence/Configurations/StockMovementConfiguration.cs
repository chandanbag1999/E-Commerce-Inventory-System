using EcommerceInventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceInventory.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.MovementType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sm => sm.Quantity)
            .IsRequired();

        builder.Property(sm => sm.QuantityBefore)
            .IsRequired();

        builder.Property(sm => sm.QuantityAfter)
            .IsRequired();

        builder.Property(sm => sm.ReferenceType)
            .HasMaxLength(50);

        builder.Property(sm => sm.Notes)
            .HasColumnType("text");

        builder.HasOne(sm => sm.Stock)
            .WithMany(s => s.Movements)
            .HasForeignKey(sm => sm.StockId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sm => sm.StockId)
            .HasDatabaseName("idx_stock_movements_stock");

        builder.HasIndex(sm => sm.ReferenceId)
            .HasDatabaseName("idx_stock_movements_ref");
    }
}
