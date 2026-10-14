using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SportUp.Entities;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SportUp.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public (string token, DateTime expiresAt) GenerateAccessToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");

        // Dùng Fallback an toàn nếu AppSettings bị Null trên môi trường Deploy
        var secret = jwtSettings["SecretKey"] ?? "SportUp_Super_Secret_Key_For_Jwt_Authentication_2026_Minimum_32_Bytes!";
        var secretKey = Encoding.UTF8.GetBytes(secret);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role ?? "Viewer")
        };

        double minutes = 60;
        if (double.TryParse(jwtSettings["AccessTokenExpirationMinutes"], out var parsedMinutes))
        {
            minutes = parsedMinutes;
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(minutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = jwtSettings["Issuer"] ?? "SportUpIssuer",
            Audience = jwtSettings["Audience"] ?? "SportUpAudience",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKey),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(securityToken), expiresAt);
    }

    public RefreshToken GenerateRefreshToken(int userId)
    {
        var jwtSettings = _config.GetSection("JwtSettings");

        double days = 7;
        if (double.TryParse(jwtSettings["RefreshTokenExpirationDays"], out var parsedDays))
        {
            days = parsedDays;
        }

        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(randomNumber),
            ExpiresAt = DateTime.UtcNow.AddDays(days),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
    }
}