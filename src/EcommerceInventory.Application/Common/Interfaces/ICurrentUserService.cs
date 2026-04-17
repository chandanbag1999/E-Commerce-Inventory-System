namespace EcommerceInventory.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid?   UserId          { get; }
    string? Email           { get; }
    string? FullName        { get; }
    bool    IsAuthenticated { get; }
    IEnumerable<string> Roles       { get; }
    IEnumerable<string> Permissions { get; }
    bool HasPermission(string permission);
    bool HasRole(string role);
}
