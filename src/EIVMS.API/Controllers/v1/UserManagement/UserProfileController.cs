using System.Security.Claims;
using EIVMS.Application.Modules.UserManagement.DTOs.User;
using EIVMS.Application.Modules.UserManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EIVMS.API.Controllers.v1.UserManagement;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _profileService;

    public UserProfileController(IUserProfileService profileService)
    {
        _profileService = profileService;
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(claim!);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        var result = await _profileService.GetProfileAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _profileService.UpdateProfileAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("profile/picture")]
    [RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var userId = GetCurrentUserId();
        var result = await _profileService.UploadProfilePictureAsync(userId, file);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _profileService.ChangePasswordAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("notifications")]
    public async Task<IActionResult> UpdateNotifications([FromBody] UpdateNotificationPreferencesDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _profileService.UpdateNotificationPreferencesAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("gdpr/export")]
    public async Task<IActionResult> RequestDataExport()
    {
        var userId = GetCurrentUserId();
        var result = await _profileService.RequestDataExportAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("gdpr/delete")]
    public async Task<IActionResult> RequestDataDeletion()
    {
        var userId = GetCurrentUserId();
        var result = await _profileService.RequestDataDeletionAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Authorize(Policy = "user:manage")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int size = 20, [FromQuery] string? search = null, [FromQuery] string? status = null)
    {
        var result = await _profileService.GetUsersListAsync(page, size, search, status);
        return Ok(result);
    }

    [HttpGet("{id:guid}/profile")]
    [Authorize(Policy = "user:view")]
    public async Task<IActionResult> GetUserProfile(Guid id)
    {
        var result = await _profileService.GetProfileAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id:guid}/suspend")]
    [Authorize(Policy = "user:manage")]
    public async Task<IActionResult> SuspendUser(Guid id, [FromBody] SuspendUserDto dto)
    {
        var adminId = GetCurrentUserId();
        var result = await _profileService.SuspendUserAsync(id, dto.Reason, adminId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Policy = "user:manage")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var adminId = GetCurrentUserId();
        var result = await _profileService.ActivateUserAsync(id, adminId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "user:manage")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var adminId = GetCurrentUserId();
        var result = await _profileService.DeleteUserAsync(id, adminId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

public class SuspendUserDto
{
    public string Reason { get; set; } = string.Empty;
}