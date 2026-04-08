using EIVMS.Domain.Common;

namespace EIVMS.Domain.Entities.Identity;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;

    private RolePermission() { }

    public static RolePermission Create(Guid roleId, Guid permissionId)
    {
        return new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAt = DateTime.UtcNow
        };
    }
}