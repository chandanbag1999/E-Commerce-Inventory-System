namespace EIVMS.Application.Modules.UserManagement.DTOs.User;

public class UpdateProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public string Language { get; set; } = "en";
    public string Currency { get; set; } = "INR";
    public string TimeZone { get; set; } = "Asia/Kolkata";
}