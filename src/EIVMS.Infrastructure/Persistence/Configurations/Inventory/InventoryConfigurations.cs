using EIVMS.Domain.Entities.Inventory;
using EIVMS.Domain.Enums.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations.Inventory;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Code).IsRequired().HasMaxLength(50);
        builder.Property(w => w.Description).HasMaxLength(500);
        builder.Property(w => w.Address).IsRequired().HasMaxLength(500);
        builder.Property(w => w.City).IsRequired().HasMaxLength(100);
        builder.Property(w => w.State).IsRequired().HasMaxLength(100);
        builder.Property(w => w.Country).IsRequired().HasMaxLength(100);
        builder.Property(w => w.PinCode).IsRequired().HasMaxLength(20);
        builder.Property(w => w.Latitude).HasPrecision(10, 7);
        builder.Property(w => w.Longitude).HasPrecision(10, 7);
        builder.Property(w => w.ContactPerson).HasMaxLength(200);
        builder.Property(w => w.ContactPhone).HasMaxLength(50);
        builder.Property(w => w.ContactEmail).HasMaxLength(200);
        builder.Property(w => w.MaxCapacity).HasPrecision(18, 2);
        builder.Property(w => w.Status).HasConversion<int>().HasDefaultValue(WarehouseStatus.Active);
        builder.Property(w => w.IsDefault).HasDefaultValue(false);
        builder.Property(w => w.IsDeleted).HasDefaultValue(false);
        builder.Property(w => w.Priority).HasDefaultValue(1);
        builder.HasIndex(w => w.Code).IsUnique();
        builder.HasIndex(w => w.IsDeleted);
        builder.HasIndex(w => w.Status);
    }
}

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.SKU).IsRequired().HasMaxLength(100);
        builder.Property(i => i.TotalQuantity).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(i => i.ReservedQuantity).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(i => i.DamagedQuantity).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(i => i.InTransitQuantity).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(i => i.LowStockThreshold).HasDefaultValue(10);
        builder.Property(i => i.MaxStockLevel).HasPrecision(18, 2);
        builder.Property(i => i.ReorderPoint).HasDefaultValue(5);
        builder.Property(i => i.ReorderQuantity).HasPrecision(18, 2);
        builder.Property(i => i.AverageCost).HasPrecision(18, 4);
        builder.Property(i => i.LastCost).HasPrecision(18, 4);
        builder.Property(i => i.Version).IsConcurrencyToken();
        builder.Property(i => i.BatchNumber).HasMaxLength(100);
        builder.Property(i => i.Location).HasMaxLength(200);
        builder.Property(i => i.IsActive).HasDefaultValue(true);
        builder.HasIndex(i => new { i.SKU, i.WarehouseId });
        builder.HasIndex(i => i.ProductId);
        builder.HasIndex(i => i.WarehouseId);
        builder.HasIndex(i => i.IsActive);
        builder.HasOne(i => i.Warehouse).WithMany(w => w.InventoryItems).HasForeignKey(i => i.WarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.SKU).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Quantity).IsRequired().HasPrecision(18, 2);
        builder.Property(m => m.ReferenceId).HasMaxLength(100);
        builder.Property(m => m.ReferenceType).HasMaxLength(50);
        builder.Property(m => m.Notes).HasMaxLength(500);
        builder.Property(m => m.Reason).HasMaxLength(200);
        builder.Property(m => m.PerformedByName).HasMaxLength(100);
        builder.Property(m => m.UnitCost).HasPrecision(18, 4);
        builder.Property(m => m.TotalCost).HasPrecision(18, 4);
        builder.Property(m => m.Type).HasConversion<int>().IsRequired();
        builder.HasIndex(m => new { m.SKU, m.WarehouseId, m.CreatedAt });
        builder.HasIndex(m => m.ReferenceId);
        builder.HasOne(m => m.InventoryItem).WithMany().HasForeignKey(m => m.InventoryItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Warehouse).WithMany().HasForeignKey(m => m.WarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("StockReservations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.SKU).IsRequired().HasMaxLength(100);
        builder.Property(r => r.OrderId).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Quantity).IsRequired().HasPrecision(18, 2);
        builder.Property(r => r.Status).HasConversion<int>().HasDefaultValue(StockReservationStatus.Pending);
        builder.Property(r => r.ReleaseReason).HasMaxLength(300);
        builder.Property(r => r.UnitPrice).HasPrecision(18, 2);
        builder.Property(r => r.TotalPrice).HasPrecision(18, 2);
        builder.HasIndex(r => new { r.OrderId, r.Status });
        builder.HasIndex(r => new { r.ExpiresAt, r.Status });
        builder.HasOne(r => r.InventoryItem).WithMany().HasForeignKey(r => r.InventoryItemId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("StockTransfers");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.SKU).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Quantity).IsRequired().HasPrecision(18, 2);
        builder.Property(t => t.TransferNumber).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Status).HasMaxLength(20).HasDefaultValue("Pending");
        builder.Property(t => t.Notes).HasMaxLength(1000);
        builder.HasIndex(t => t.TransferNumber).IsUnique();
        builder.HasIndex(t => t.Status);
        builder.HasOne(t => t.SourceWarehouse).WithMany().HasForeignKey(t => t.SourceWarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.DestinationWarehouse).WithMany().HasForeignKey(t => t.DestinationWarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class InventoryAlertConfiguration : IEntityTypeConfiguration<InventoryAlert>
{
    public void Configure(EntityTypeBuilder<InventoryAlert> builder)
    {
        builder.ToTable("InventoryAlerts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.SKU).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Message).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.Type).HasConversion<int>().IsRequired();
        builder.Property(a => a.IsNotificationSent).HasDefaultValue(false);
        builder.HasIndex(a => a.InventoryItemId);
        builder.HasIndex(a => a.WarehouseId);
        builder.HasIndex(a => a.Type);
        builder.HasIndex(a => a.IsResolved);
    }
}