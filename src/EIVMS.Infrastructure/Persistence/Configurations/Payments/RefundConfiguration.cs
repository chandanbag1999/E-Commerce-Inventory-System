using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EIVMS.Domain.Entities.Payments;
using EIVMS.Domain.Enums.Payments;

namespace EIVMS.Infrastructure.Persistence.Configurations.Payments;

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("refunds");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.RefundNumber)
            .HasColumnName("refund_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(r => r.RequestedByUserId)
            .HasColumnName("requested_by_user_id");

        builder.Property(r => r.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<RefundStatus>(v))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Reason)
            .HasColumnName("reason")
            .HasMaxLength(500);

        builder.Property(r => r.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(r => r.ProviderRefundId)
            .HasColumnName("provider_refund_id")
            .HasMaxLength(100);

        builder.Property(r => r.GatewayResponse)
            .HasColumnName("gateway_response")
            .HasColumnType("text");

        builder.Property(r => r.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(500);

        builder.Property(r => r.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(r => r.IsFullRefund)
            .HasColumnName("is_full_refund")
            .HasDefaultValue(false);

        builder.HasIndex(r => r.RefundNumber)
            .IsUnique()
            .HasDatabaseName("ix_refunds_refund_number");

        builder.HasIndex(r => r.PaymentId)
            .HasDatabaseName("ix_refunds_payment_id");

        builder.HasIndex(r => r.ProviderRefundId)
            .HasDatabaseName("ix_refunds_provider_refund_id");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("ix_refunds_status");

        builder.HasIndex(r => r.CreatedAt)
            .HasDatabaseName("ix_refunds_created_at");
    }
}