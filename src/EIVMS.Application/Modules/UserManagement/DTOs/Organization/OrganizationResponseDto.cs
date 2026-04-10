using EIVMS.Domain.Enums;

namespace EIVMS.Application.Modules.UserManagement.DTOs.Organization;

public class OrganizationResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public OrganizationType Type { get; set; }
    public OrganizationStatus Status { get; set; }
    public string? GstNumber { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Country { get; set; }
    public string? PrimaryColor { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
}