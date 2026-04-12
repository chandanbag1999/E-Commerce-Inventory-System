using EIVMS.Domain.Entities.Identity;

namespace EIVMS.Application.Modules.Identity.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithRolesAsync(Guid userId);
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task AddUserRoleAsync(UserRole userRole);
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);
    Task RevokeTokenFamilyAsync(string tokenFamily);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
    Task<Role?> GetRoleByNameAsync(string roleName);
    Task<List<User>> GetUsersByRoleNamesAsync(params string[] roleNames);
}
