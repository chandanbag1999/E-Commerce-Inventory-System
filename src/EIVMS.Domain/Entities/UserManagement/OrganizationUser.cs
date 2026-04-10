using EIVMS.Domain.Common;

namespace EIVMS.Domain.Entities.UserManagement;

public class OrganizationUser : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public string? Designation { get; private set; }
    public string? Department { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsOrganizationAdmin { get; private set; } = false;

    public Organization Organization { get; private set; } = null!;
    public Identity.User User { get; private set; } = null!;

    private OrganizationUser() { }

    public static OrganizationUser Create(
        Guid organizationId,
        Guid userId,
        string? designation = null,
        string? department = null,
        bool isAdmin = false)
    {
        return new OrganizationUser
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            UserId = userId,
            Designation = designation,
            Department = department,
            IsOrganizationAdmin = isAdmin,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void MakeAdmin()
    {
        IsOrganizationAdmin = true;
        SetUpdatedAt();
    }

    public void RemoveAdmin()
    {
        IsOrganizationAdmin = false;
        SetUpdatedAt();
    }

    public void UpdateDesignation(string? designation, string? department)
    {
        Designation = designation;
        Department = department;
        SetUpdatedAt();
    }
}