using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EcommerceInventory.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EcommerceInventory.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var userIdStr = User?.FindFirstValue("userId")
                         ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirstValue(JwtRegisteredClaimNames.Email) ?? User?.FindFirstValue(ClaimTypes.Email);
    public string? FullName => User?.FindFirstValue("fullName") ?? User?.FindFirstValue(ClaimTypes.Name);
    public IEnumerable<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
    public IEnumerable<string> Permissions => User?.FindAll("permission").Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool HasPermission(string permission) => Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    public bool HasRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}