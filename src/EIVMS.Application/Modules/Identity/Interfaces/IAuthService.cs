using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.Identity.DTOs;

namespace EIVMS.Application.Modules.Identity.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto);
    Task<ApiResponse<bool>> LogoutAsync(string refreshToken);
    Task<ApiResponse<CurrentUserDto>> GetCurrentUserAsync(Guid userId);
}