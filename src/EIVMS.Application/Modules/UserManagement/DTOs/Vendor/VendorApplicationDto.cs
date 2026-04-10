using EIVMS.Domain.Enums;

namespace EIVMS.Application.Modules.UserManagement.DTOs.Vendor;

public class CreateVendorApplicationDto
{
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string? BusinessDescription { get; set; }
    public string? GstNumber { get; set; }
    public string? BusinessLicenseNumber { get; set; }
    public string Country { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class UpdateBankDetailsDto
{
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankIfscCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
}

public class ReviewVendorApplicationDto
{
    public bool IsApproved { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
}

public class VendorApplicationResponseDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public VendorOnboardingStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public string? GstNumber { get; set; }
    public string? ReviewNotes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserName { get; set; }
    public int ResubmissionCount { get; set; }
    public bool HasGstDocument { get; set; }
    public bool HasBusinessLicense { get; set; }
    public bool HasPanCard { get; set; }
    public bool HasBankStatement { get; set; }
    public bool HasBankDetails { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<VendorAuditLogDto> AuditLogs { get; set; } = new();
}

public class VendorAuditLogDto
{
    public string Action { get; set; } = string.Empty;
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
}