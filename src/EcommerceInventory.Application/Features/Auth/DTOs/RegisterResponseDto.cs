namespace EcommerceInventory.Application.Features.Auth.DTOs;

public class RegisterResponseDto
{
    public Guid     Id               { get; set; }
    public string   FullName         { get; set; } = string.Empty;
    public string   Email            { get; set; } = string.Empty;
    public string?  Phone            { get; set; }
    public string   Status           { get; set; } = string.Empty;
    public bool     IsEmailVerified  { get; set; }
    public DateTime CreatedAt        { get; set; }
}