using EIVMS.Domain.Common;

namespace EIVMS.Domain.Entities.UserManagement;

public class UserAuditLog : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public bool IsSuccessful { get; private set; }
    public string? FailureReason { get; private set; }

    public Identity.User User { get; private set; } = null!;

    private UserAuditLog() { }

    public static UserAuditLog Create(
        Guid userId,
        string action,
        bool isSuccessful = true,
        string? description = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? oldValue = null,
        string? newValue = null,
        string? failureReason = null)
    {
        return new UserAuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action.ToUpperInvariant(),
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            OldValue = oldValue,
            NewValue = newValue,
            IsSuccessful = isSuccessful,
            FailureReason = failureReason,
            CreatedAt = DateTime.UtcNow
        };
    }
}