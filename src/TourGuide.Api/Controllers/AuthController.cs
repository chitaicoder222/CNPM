using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TourGuide.Api.Data;
using TourGuide.Api.Data.Entities;

namespace TourGuide.Api.Controllers;

public record RegisterRequest(string Email, string Password, string? Role);
public record LoginRequest(string Email, string Password);

[ApiController]
[Route("auth")]
public class AuthController(IntegrationDbContext db, IConfiguration config) : ControllerBase
{
    private readonly PasswordHasher<AppUser> _hasher = new();

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || request.Password.Length < 8) return BadRequest("Email và mật khẩu (tối thiểu 8 ký tự) là bắt buộc.");
        if (await db.Users.AnyAsync(x => x.Email == email, ct)) return Conflict("Email đã tồn tại.");
        // Self-registration can never grant elevated privileges.
        var user = new AppUser { Email = email, Role = "tourist" };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        db.Users.Add(user); await db.SaveChangesAsync(ct);
        return Created("auth/me", new { user.Id, user.Email, user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed) return Unauthorized("Email hoặc mật khẩu không đúng.");
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.Role, user.Role) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
        var token = new JwtSecurityToken(config["Jwt:Issuer"] ?? "TourGuide", null, claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), user = new { user.Id, user.Email, user.Role } });
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var id = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(id, out var userId) && await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, ct) is { } user
            ? Ok(new { user.Id, user.Email, user.Role }) : Unauthorized();
    }
}
