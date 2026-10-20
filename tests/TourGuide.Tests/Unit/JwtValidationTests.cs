using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace TourGuide.Tests.Unit;

public class JwtValidationTests
{
    private const string Secret = "unit-test-secret-at-least-32-characters-long";

    private static string CreateToken(string role, string secret = Secret)
    {
        var claims = new[]
        {
            new Claim("sub", "1"),
            new Claim(ClaimTypes.Email, "admin@test.com"),
            new Claim(ClaimTypes.Role, role)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var token = new JwtSecurityToken("TourGuide", null, claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static TokenValidationParameters Parameters() => new()
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "TourGuide",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret))
    };

    [Fact]
    public async Task ValidateToken_AcceptsIssuedAdminToken()
    {
        var result = await new JsonWebTokenHandler().ValidateTokenAsync(CreateToken("admin"), Parameters());
        Assert.True(result.IsValid, result.Exception?.Message);
        Assert.Equal("admin", result.ClaimsIdentity!.FindFirst(ClaimTypes.Role)?.Value);
    }

    [Fact]
    public async Task ValidateToken_RejectsDifferentSecret()
    {
        var result = await new JsonWebTokenHandler().ValidateTokenAsync(CreateToken("admin", "another-secret-at-least-32-characters-long!"), Parameters());
        Assert.False(result.IsValid);
    }

    [Fact]
    public void IdentityModelPackages_ShareVersion()
    {
        var tokens = typeof(SecurityToken).Assembly.GetName().Version;
        Assert.Equal(tokens, typeof(JsonWebToken).Assembly.GetName().Version);
        Assert.Equal(tokens, typeof(JwtSecurityToken).Assembly.GetName().Version);
    }
}
