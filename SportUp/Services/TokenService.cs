using Microsoft.IdentityModel.Tokens;
using SportUp.Entities;
using SportUp.Entities;
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

    // Sinh AccessToken (JWT) chứa thông tin Claims người dùng
    public (string token, DateTime expiresAt) GenerateAccessToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["AccessTokenExpirationMinutes"]!));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKey),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(securityToken), expiresAt);
    }

    // Sinh ngẫu nhiên chuỗi Refresh Token ngẫu nhiên (Cryptographic)
    public RefreshToken GenerateRefreshToken(int userId)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var days = double.Parse(jwtSettings["RefreshTokenExpirationDays"]!);

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