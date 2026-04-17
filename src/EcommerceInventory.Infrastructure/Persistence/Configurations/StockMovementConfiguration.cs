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
        builder.Property(sm => sm.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(sm => sm.StockId).HasColumnName("stock_id").IsRequired();
        builder.Property(sm => sm.MovementType).HasColumnName("movement_type").HasMaxLength(50).IsRequired();
        builder.Property(sm => sm.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(sm => sm.QuantityBefore).HasColumnName("quantity_before").IsRequired();
        builder.Property(sm => sm.QuantityAfter).HasColumnName("quantity_after").IsRequired();
        builder.Property(sm => sm.ReferenceId).HasColumnName("reference_id");
        builder.Property(sm => sm.ReferenceType).HasColumnName("reference_type").HasMaxLength(50);
        builder.Property(sm => sm.Notes).HasColumnName("notes").HasColumnType("text");
        builder.Property(sm => sm.PerformedBy).HasColumnName("performed_by");
        builder.Property(sm => sm.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Ignore(sm => sm.UpdatedAt);

        builder.HasIndex(sm => sm.StockId).HasDatabaseName("idx_stock_movements_stock");
        builder.HasIndex(sm => sm.ReferenceId).HasDatabaseName("idx_stock_movements_ref");
    }
}