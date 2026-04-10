using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EIVMS.Domain.Entities.Payments;
using EIVMS.Domain.Enums.Payments;

namespace EIVMS.Infrastructure.Persistence.Configurations.Payments;

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("payment_attempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(a => a.FromStatus)
            .HasColumnName("from_status")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<PaymentStatus>(v))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.ToStatus)
            .HasColumnName("to_status")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<PaymentStatus>(v))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.Reason)
            .HasColumnName("reason")
            .HasMaxLength(500);

        builder.Property(a => a.GatewayResponse)
            .HasColumnName("gateway_response")
            .HasColumnType("text");

        builder.Property(a => a.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(a => a.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        builder.HasIndex(a => a.PaymentId)
            .HasDatabaseName("ix_payment_attempts_payment_id");

        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("ix_payment_attempts_created_at");
    }
}