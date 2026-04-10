using EIVMS.Domain.Common;
using EIVMS.Domain.Enums;

namespace EIVMS.Domain.Entities.UserManagement;

public class VendorApplication : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid ApplicantUserId { get; private set; }
    public VendorOnboardingStatus Status { get; private set; } = VendorOnboardingStatus.Draft;
    public string BusinessName { get; private set; } = string.Empty;
    public string BusinessType { get; private set; } = string.Empty;
    public string? BusinessDescription { get; private set; }
    public string? GstNumber { get; private set; }
    public string? BusinessLicenseNumber { get; private set; }
    public string? BankAccountNumber { get; private set; }
    public string? BankIfscCode { get; private set; }
    public string? BankName { get; private set; }
    public string? AccountHolderName { get; private set; }
    public string? GstDocumentUrl { get; private set; }
    public string? BusinessLicenseUrl { get; private set; }
    public string? PanCardUrl { get; private set; }
    public string? BankStatementUrl { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewNotes { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public int ResubmissionCount { get; private set; } = 0;

    public Organization Organization { get; private set; } = null!;
    public Identity.User ApplicantUser { get; private set; } = null!;
    public ICollection<VendorApplicationAuditLog> AuditLogs { get; private set; } = new List<VendorApplicationAuditLog>();

    private VendorApplication() { }

    public static VendorApplication Create(
        Guid organizationId,
        Guid applicantUserId,
        string businessName,
        string businessType,
        string? businessDescription = null)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name is required");

        return new VendorApplication
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ApplicantUserId = applicantUserId,
            BusinessName = businessName.Trim(),
            BusinessType = businessType.Trim(),
            BusinessDescription = businessDescription,
            Status = VendorOnboardingStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Submit()
    {
        if (Status != VendorOnboardingStatus.Draft &&
            Status != VendorOnboardingStatus.DocumentsRequired)
            throw new InvalidOperationException($"Cannot submit from status: {Status}");

        Status = VendorOnboardingStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        if (Status == VendorOnboardingStatus.DocumentsRequired)
            ResubmissionCount++;
        SetUpdatedAt();
    }

    public void StartReview(Guid reviewerUserId)
    {
        if (Status != VendorOnboardingStatus.Submitted)
            throw new InvalidOperationException("Can only start review for submitted applications");

        Status = VendorOnboardingStatus.UnderReview;
        ReviewedByUserId = reviewerUserId;
        SetUpdatedAt();
    }

    public void RequestDocuments(string notes)
    {
        if (Status != VendorOnboardingStatus.UnderReview)
            throw new InvalidOperationException("Can only request documents during review");

        Status = VendorOnboardingStatus.DocumentsRequired;
        ReviewNotes = notes;
        SetUpdatedAt();
    }

    public void Approve(Guid reviewerUserId, string? notes = null)
    {
        if (Status != VendorOnboardingStatus.UnderReview)
            throw new InvalidOperationException("Can only approve applications under review");

        Status = VendorOnboardingStatus.Approved;
        ReviewedByUserId = reviewerUserId;
        ReviewedAt = DateTime.UtcNow;
        ReviewNotes = notes;
        SetUpdatedAt();
    }

    public void Reject(Guid reviewerUserId, string reason)
    {
        if (Status != VendorOnboardingStatus.UnderReview)
            throw new InvalidOperationException("Can only reject applications under review");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required");

        Status = VendorOnboardingStatus.Rejected;
        ReviewedByUserId = reviewerUserId;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = reason;
        SetUpdatedAt();
    }

    public void UpdateBusinessDetails(
        string businessName,
        string businessType,
        string? description,
        string? gstNumber,
        string? licenseNumber)
    {
        if (Status != VendorOnboardingStatus.Draft &&
            Status != VendorOnboardingStatus.DocumentsRequired)
            throw new InvalidOperationException("Can only update in Draft or DocumentsRequired status");

        BusinessName = businessName;
        BusinessType = businessType;
        BusinessDescription = description;
        GstNumber = gstNumber;
        BusinessLicenseNumber = licenseNumber;
        SetUpdatedAt();
    }

    public void UpdateBankDetails(
        string accountNumber,
        string ifscCode,
        string bankName,
        string accountHolderName)
    {
        BankAccountNumber = accountNumber;
        BankIfscCode = ifscCode;
        BankName = bankName;
        AccountHolderName = accountHolderName;
        SetUpdatedAt();
    }

    public void UpdateDocuments(
        string? gstDocUrl,
        string? licenseUrl,
        string? panCardUrl,
        string? bankStatementUrl)
    {
        GstDocumentUrl = gstDocUrl ?? GstDocumentUrl;
        BusinessLicenseUrl = licenseUrl ?? BusinessLicenseUrl;
        PanCardUrl = panCardUrl ?? PanCardUrl;
        BankStatementUrl = bankStatementUrl ?? BankStatementUrl;
        SetUpdatedAt();
    }
}