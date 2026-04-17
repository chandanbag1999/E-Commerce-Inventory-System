namespace EcommerceInventory.Infrastructure.Settings;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpMinutes { get; set; } = 15;
    public int RefreshTokenExpDays { get; set; } = 7;
}