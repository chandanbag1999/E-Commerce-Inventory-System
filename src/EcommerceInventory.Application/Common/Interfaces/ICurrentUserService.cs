namespace EcommerceInventory.Application.Common.Interfaces;

/// <summary>
/// Service to get current user information from HTTP context
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    IList<string> Roles { get; }
    IList<string> Permissions { get; }
    bool IsAuthenticated { get; }
    bool HasPermission(string permission);
    bool HasRole(string role);
}
