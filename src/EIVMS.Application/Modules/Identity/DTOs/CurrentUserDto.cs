namespace EIVMS.Application.Modules.Identity.DTOs;

public class CurrentUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public string Role { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
}