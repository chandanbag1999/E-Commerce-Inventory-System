namespace EcommerceInventory.Application.Features.Auth.DTOs;

public class LoginResponseDto
{
    public string      AccessToken  { get; set; } = string.Empty;
    public string      RefreshToken { get; set; } = string.Empty;
    public int         ExpiresIn    { get; set; }
    public string      TokenType    { get; set; } = "Bearer";
    public UserInfoDto User         { get; set; } = null!;
}

public class UserInfoDto
{
    public Guid                 Id               { get; set; }
    public string               FullName         { get; set; } = string.Empty;
    public string               Email            { get; set; } = string.Empty;
    public string?              Phone            { get; set; }
    public string?              ProfileImageUrl  { get; set; }
    public string               Status           { get; set; } = string.Empty;
    public bool                 IsEmailVerified  { get; set; }
    public IEnumerable<string>  Roles            { get; set; } = new List<string>();
    public IEnumerable<string>  Permissions      { get; set; } = new List<string>();
    public DateTime?            LastLoginAt      { get; set; }
    public DateTime             CreatedAt        { get; set; }
}