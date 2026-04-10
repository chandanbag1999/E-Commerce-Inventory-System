using EIVMS.Domain.Common;
using EIVMS.Domain.Enums;

namespace EIVMS.Domain.Entities.UserManagement;

public class Organization : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public OrganizationType Type { get; private set; }
    public OrganizationStatus Status { get; private set; }
    public string? GstNumber { get; private set; }
    public string? PanNumber { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? Country { get; private set; }
    public string? State { get; private set; }
    public string? City { get; private set; }
    public string? PrimaryColor { get; private set; }
    public string? SecondaryColor { get; private set; }
    public string? CustomDomain { get; private set; }
    public bool IsDeleted { get; private set; } = false;
    public DateTime? DeletedAt { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    public ICollection<OrganizationUser> OrganizationUsers { get; private set; } = new List<OrganizationUser>();
    public VendorApplication? VendorApplication { get; private set; }

    private Organization() { }

    public static Organization Create(
        string name,
        OrganizationType type,
        Guid? createdByUserId = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name is required");

        return new Organization
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description,
            Type = type,
            Status = OrganizationStatus.PendingVerification,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Activate()
    {
        Status = OrganizationStatus.Active;
        SetUpdatedAt();
    }

    public void Suspend(string reason)
    {
        Status = OrganizationStatus.Suspended;
        SetUpdatedAt();
    }

    public void UpdateBusinessInfo(
        string? gstNumber,
        string? panNumber,
        string? websiteUrl,
        string? contactEmail,
        string? contactPhone)
    {
        GstNumber = gstNumber;
        PanNumber = panNumber;
        WebsiteUrl = websiteUrl;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        SetUpdatedAt();
    }

    public void UpdateBranding(
        string? logoUrl,
        string? primaryColor,
        string? secondaryColor,
        string? customDomain)
    {
        LogoUrl = logoUrl;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        CustomDomain = customDomain;
        SetUpdatedAt();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        Status = OrganizationStatus.Deleted;
        SetUpdatedAt();
    }
}