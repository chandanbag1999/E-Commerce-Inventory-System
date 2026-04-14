using System.Security.Claims;
using EcommerceInventory.Domain.Entities;

namespace EcommerceInventory.Application.Common.Interfaces;

/// <summary>
/// Service for generating and managing JWT tokens
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates an access token for the user
    /// </summary>
    string GenerateAccessToken(User user, IList<string> roles, IList<string> permissions);

    /// <summary>
    /// Generates a refresh token
    /// </summary>
    RefreshToken GenerateRefreshToken(Guid userId, string? deviceInfo = null);

    /// <summary>
    /// Extracts principal from an expired access token (for refresh flow)
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Hashes a password using BCrypt
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}
