using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EIVMS.Domain.Entities.Payments;
using EIVMS.Domain.Enums.Payments;

namespace EIVMS.Infrastructure.Persistence.Configurations.Payments;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.PaymentNumber)
            .HasColumnName("payment_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.RefundedAmount)
            .HasColumnName("refunded_amount")
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(p => p.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<PaymentStatus>(v))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.Provider)
            .HasColumnName("provider")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<PaymentProvider>(v))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.ProviderPaymentId)
            .HasColumnName("provider_payment_id")
            .HasMaxLength(100);

        builder.Property(p => p.ProviderRefundId)
            .HasColumnName("provider_refund_id")
            .HasMaxLength(100);

        builder.Property(p => p.CustomerEmail)
            .HasColumnName("customer_email")
            .HasMaxLength(255);

        builder.Property(p => p.CustomerPhone)
            .HasColumnName("customer_phone")
            .HasMaxLength(20);

        builder.Property(p => p.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(255);

        builder.Property(p => p.BillingAddress)
            .HasColumnName("billing_address")
            .HasMaxLength(500);

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(p => p.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(p => p.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(p => p.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(500);

        builder.Property(p => p.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(p => p.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(p => p.PaymentUrl)
            .HasColumnName("payment_url")
            .HasMaxLength(2000);

        builder.Property(p => p.RedirectUrl)
            .HasColumnName("redirect_url")
            .HasMaxLength(2000);

        builder.Property(p => p.WebhookPayload)
            .HasColumnName("webhook_payload")
            .HasColumnType("text");

        builder.Property(p => p.WebhookProcessedAt)
            .HasColumnName("webhook_processed_at");

        builder.Property(p => p.GatewayResponse)
            .HasColumnName("gateway_response")
            .HasColumnType("text");

        builder.HasIndex(p => p.PaymentNumber)
            .IsUnique()
            .HasDatabaseName("ix_payments_payment_number");

        builder.HasIndex(p => p.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("ix_payments_idempotency_key");

        builder.HasIndex(p => p.OrderId)
            .HasDatabaseName("ix_payments_order_id");

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("ix_payments_user_id");

        builder.HasIndex(p => p.ProviderPaymentId)
            .HasDatabaseName("ix_payments_provider_payment_id");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("ix_payments_status");

        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("ix_payments_created_at");

        builder.HasMany(p => p.Attempts)
            .WithOne(a => a.Payment)
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Refunds)
            .WithOne(r => r.Payment)
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}