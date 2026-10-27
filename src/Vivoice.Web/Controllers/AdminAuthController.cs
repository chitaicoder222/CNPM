// Purpose: Handles secure three-field administrator sign-in, sign-out and current-principal lookup.
// Responsibility: Verifies server-side hashed credentials and issues only an HTTP-only authentication cookie.
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data;
using Vivoice.Web.Data.Entities;
using Vivoice.Web.Models;
using Vivoice.Web.Services;

namespace Vivoice.Web.Controllers;

[ApiController]
[Route("api/admin/auth")]
public sealed class AdminAuthController(VivoiceDbContext db) : ControllerBase
{
    private readonly PasswordHasher<AdminUser> _hasher = new();

    [HttpPost("login")]
    public async Task<IActionResult> Login(AdminLoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { code = "VALIDATION_ERROR", message = "Tên admin, số điện thoại và mật khẩu đều bắt buộc." });
        }

        var normalizedName = IdentityNormalizer.Name(request.DisplayName);
        var normalizedPhone = IdentityNormalizer.Phone(request.Phone);
        var admin = await db.AdminUsers.FirstOrDefaultAsync(x => x.NormalizedDisplayName == normalizedName && x.NormalizedPhone == normalizedPhone && x.IsActive, cancellationToken);
        if (admin is null || _hasher.VerifyHashedPassword(admin, admin.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { code = "INVALID_CREDENTIALS", message = "Thông tin đăng nhập không hợp lệ." });
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name, admin.DisplayName),
            new Claim(ClaimTypes.Role, "admin")
        ], CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = false, AllowRefresh = true });
        db.AuditLogs.Add(new AuditLog { AdminUserId = admin.Id, Action = "AUTH_LOGIN", EntityType = "AdminUser", EntityId = admin.Id.ToString() });
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { admin = new { admin.Id, admin.DisplayName } });
    }

    [Authorize(Roles = "admin")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new { displayName = User.Identity?.Name });
}
