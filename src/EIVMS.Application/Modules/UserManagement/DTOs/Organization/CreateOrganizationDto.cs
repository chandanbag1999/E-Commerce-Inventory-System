using EIVMS.Domain.Enums;

namespace EIVMS.Application.Modules.UserManagement.DTOs.Organization;

public class CreateOrganizationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public OrganizationType Type { get; set; } = OrganizationType.Vendor;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? WebsiteUrl { get; set; }
}