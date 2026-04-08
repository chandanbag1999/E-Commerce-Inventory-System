using EIVMS.Domain.Common;

namespace EIVMS.Domain.Entities.Identity;

public class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole() { }

    public static UserRole Create(Guid userId, Guid roleId)
    {
        return new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        };
    }
}