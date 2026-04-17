namespace EcommerceInventory.Application.Features.Auth.DTOs;

public class TokenDto
{
    public string AccessToken  { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int    ExpiresIn    { get; set; }
    public string TokenType    { get; set; } = "Bearer";
}