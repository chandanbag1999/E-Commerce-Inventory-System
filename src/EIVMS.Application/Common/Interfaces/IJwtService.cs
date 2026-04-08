using EIVMS.Domain.Entities.Identity;

namespace EIVMS.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, List<string> roles, List<string> permissions);
    string GenerateRefreshToken();
    string HashToken(string token);
    DateTime GetAccessTokenExpiry();
    DateTime GetRefreshTokenExpiry();
}