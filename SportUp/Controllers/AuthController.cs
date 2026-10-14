using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportUp.Data;
using SportUp.DTO; // 👈 Đã sửa thành SportUp.DTO (không có 's') cho khớp với dự án
using SportUp.Entities;
using SportUp.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SportUp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Đăng ký tài khoản mới (Mật khẩu tự động được Hash bằng Argon2)
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingUser = await _context.Users.AnyAsync(u => u.Username == model.Username);
            if (existingUser)
            {
                return BadRequest(new { message = "Username already exists." });
            }

            var hashedPassword = Argon2Hasher.HashPassword(model.Password);

            var user = new User
            {
                Username = model.Username,
                PasswordHash = hashedPassword,
                FullName = model.FullName,
                Role = string.IsNullOrEmpty(model.Role) ? "Viewer" : model.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully!" });
        }

        /// <summary>
        /// Đăng nhập tài khoản (Xác thực bằng Argon2)
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.IsActive);

            if (user == null || !Argon2Hasher.VerifyPassword(model.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            SetRefreshTokenCookie(refreshToken.Token, refreshToken.ExpiresAt);

            return Ok(new AuthResponseDto(accessToken, user.Username, user.FullName ?? string.Empty, user.Role, expiresAt));
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshTokenStr = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshTokenStr))
            {
                return Unauthorized(new { message = "Refresh token not found." });
            }

            var storedToken = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == refreshTokenStr);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt <= DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            storedToken.IsRevoked = true;

            var (newAccessToken, expiresAt) = _tokenService.GenerateAccessToken(storedToken.User!);
            var newRefreshToken = _tokenService.GenerateRefreshToken(storedToken.UserId);

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            SetRefreshTokenCookie(newRefreshToken.Token, newRefreshToken.ExpiresAt);

            return Ok(new AuthResponseDto(
                newAccessToken,
                storedToken.User!.Username,
                storedToken.User.FullName ?? string.Empty,
                storedToken.User.Role,
                expiresAt
            ));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshTokenStr = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshTokenStr))
            {
                var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshTokenStr);
                if (storedToken != null)
                {
                    storedToken.IsRevoked = true;
                    await _context.SaveChangesAsync();
                }
            }

            Response.Cookies.Delete("refreshToken");
            return Ok(new { message = "Logged out successfully." });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            return Ok(new UserResponseDto(user.Id, user.Username, user.FullName ?? string.Empty, user.Role));
        }

        private void SetRefreshTokenCookie(string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires,
                SameSite = SameSiteMode.Lax,
                Secure = true
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
    }
}