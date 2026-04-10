using EIVMS.Domain.Common;
using EIVMS.Domain.Enums;

namespace EIVMS.Domain.Entities.UserManagement;

public class VendorApplicationAuditLog : BaseEntity
{
    public Guid VendorApplicationId { get; private set; }
    public Guid? PerformedByUserId { get; private set; }
    public VendorOnboardingStatus FromStatus { get; private set; }
    public VendorOnboardingStatus ToStatus { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public string? IpAddress { get; private set; }

    public VendorApplication VendorApplication { get; private set; } = null!;

    private VendorApplicationAuditLog() { }

    public static VendorApplicationAuditLog Create(
        Guid vendorApplicationId,
        VendorOnboardingStatus fromStatus,
        VendorOnboardingStatus toStatus,
        string action,
        Guid? performedByUserId = null,
        string? notes = null,
        string? ipAddress = null)
    {
        return new VendorApplicationAuditLog
        {
            Id = Guid.NewGuid(),
            VendorApplicationId = vendorApplicationId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Action = action,
            PerformedByUserId = performedByUserId,
            Notes = notes,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
    }
}