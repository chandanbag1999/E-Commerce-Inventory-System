using EIVMS.Domain.Entities.Orders;
using EIVMS.Domain.Enums.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EIVMS.Infrastructure.Persistence.Configurations.Orders;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.IdempotencyKey).HasMaxLength(100).IsRequired();
        builder.Property(o => o.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30).HasDefaultValue(OrderStatus.Pending);
        builder.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(20).HasDefaultValue(PaymentStatus.Pending);
        builder.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(20);

        builder.Property(o => o.SubTotal).HasPrecision(18, 2);
        builder.Property(o => o.DiscountAmount).HasPrecision(18, 2);
        builder.Property(o => o.ShippingCharges).HasPrecision(18, 2);
        builder.Property(o => o.TaxAmount).HasPrecision(18, 2);
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);
        builder.Property(o => o.Currency).HasMaxLength(3).HasDefaultValue("INR");
        builder.Property(o => o.CouponCode).HasMaxLength(50);
        builder.Property(o => o.CouponDiscountAmount).HasPrecision(18, 2);

        builder.Property(o => o.ShippingAddressLine1).HasMaxLength(200);
        builder.Property(o => o.ShippingAddressLine2).HasMaxLength(200);
        builder.Property(o => o.ShippingCity).HasMaxLength(50);
        builder.Property(o => o.ShippingState).HasMaxLength(50);
        builder.Property(o => o.ShippingCountry).HasMaxLength(50);
        builder.Property(o => o.ShippingPinCode).HasMaxLength(20);
        builder.Property(o => o.ShippingContactName).HasMaxLength(100);
        builder.Property(o => o.ShippingContactPhone).HasMaxLength(20);

        builder.Property(o => o.PaymentTransactionId).HasMaxLength(100);
        builder.Property(o => o.PaymentGatewayResponse).HasMaxLength(500);
        builder.Property(o => o.TrackingNumber).HasMaxLength(100);
        builder.Property(o => o.CourierName).HasMaxLength(50);
        builder.Property(o => o.TrackingUrl).HasMaxLength(500);

        builder.Property(o => o.CancellationReason).HasConversion<string>().HasMaxLength(30);
        builder.Property(o => o.CancellationNotes).HasMaxLength(500);
        builder.Property(o => o.ReturnReason).HasConversion<string>().HasMaxLength(30);
        builder.Property(o => o.ReturnNotes).HasMaxLength(500);
        builder.Property(o => o.CustomerNotes).HasMaxLength(1000);
        builder.Property(o => o.InternalNotes).HasMaxLength(1000);
        builder.Property(o => o.IsGift).HasDefaultValue(false);
        builder.Property(o => o.GiftMessage).HasMaxLength(500);
        builder.Property(o => o.InvoiceNumber).HasMaxLength(50);
        builder.Property(o => o.InvoiceUrl).HasMaxLength(500);

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.IdempotencyKey);
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PaymentStatus);
        builder.HasIndex(o => o.CreatedAt);

        builder.HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.StatusHistory)
            .WithOne(osh => osh.Order)
            .HasForeignKey(osh => osh.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.ReturnItems)
            .WithOne(ori => ori.Order)
            .HasForeignKey(ori => ori.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.OrderId).IsRequired();
        builder.Property(oi => oi.ProductId).IsRequired();
        builder.Property(oi => oi.SKU).HasMaxLength(50).IsRequired();
        builder.Property(oi => oi.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(oi => oi.VariantName).HasMaxLength(100);
        builder.Property(oi => oi.ProductImageUrl).HasMaxLength(500);
        builder.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
        builder.Property(oi => oi.DiscountedPrice).HasPrecision(18, 2);
        builder.Property(oi => oi.Quantity).IsRequired();
        builder.Property(oi => oi.TaxRate).HasPrecision(5, 2);
        builder.Property(oi => oi.TaxAmount).HasPrecision(18, 2);
        builder.Property(oi => oi.WarehouseName).HasMaxLength(100);
        builder.Property(oi => oi.ReturnedQuantity).HasDefaultValue(0);

        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.ProductId);
        builder.HasIndex(oi => oi.SKU);
    }
}

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistories");

        builder.HasKey(osh => osh.Id);

        builder.Property(osh => osh.OrderId).IsRequired();
        builder.Property(osh => osh.FromStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(osh => osh.ToStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(osh => osh.Notes).HasMaxLength(500);
        builder.Property(osh => osh.IsCustomerVisible).HasDefaultValue(true);

        builder.HasIndex(osh => osh.OrderId);
        builder.HasIndex(osh => osh.CreatedAt);
    }
}

public class OrderReturnItemConfiguration : IEntityTypeConfiguration<OrderReturnItem>
{
    public void Configure(EntityTypeBuilder<OrderReturnItem> builder)
    {
        builder.ToTable("OrderReturnItems");

        builder.HasKey(ori => ori.Id);

        builder.Property(ori => ori.OrderId).IsRequired();
        builder.Property(ori => ori.OrderItemId).IsRequired();
        builder.Property(ori => ori.Reason).HasConversion<string>().HasMaxLength(30);
        builder.Property(ori => ori.Quantity).IsRequired();
        builder.Property(ori => ori.RefundAmount).HasPrecision(18, 2);
        builder.Property(ori => ori.Notes).HasMaxLength(500);
        builder.Property(ori => ori.ProofImageUrl).HasMaxLength(500);
        builder.Property(ori => ori.Status).HasMaxLength(20).HasDefaultValue("Pending");
        builder.Property(ori => ori.AdminNotes).HasMaxLength(500);

        builder.HasIndex(ori => ori.OrderId);
        builder.HasIndex(ori => ori.OrderItemId);
    }
}