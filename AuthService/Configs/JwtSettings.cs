using Microsoft.IdentityModel.Tokens;

namespace AuthService.Configs
{
    public class JwtSettings
    {
        public required string Issuer { get; init; }
        public required string Audience { get; init; }
        public required string SecretKey { get; init; }
        public int ExpiryMinutes { get; init; }
        public SymmetricSecurityKey GetSymmetricSecurityKey() => new(System.Text.Encoding.UTF8.GetBytes(SecretKey));
    }
}