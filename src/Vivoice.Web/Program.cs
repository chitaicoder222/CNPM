// Purpose: Configures the VIVOICE web host, dependency injection, authentication and HTTP pipeline.
// Responsibility: Keeps public static UI, protected admin APIs and SQL Server access in one modular-monolith host.
// Important: Secrets and the SQL connection string come only from User Secrets or environment variables.
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Vivoice.Web.Data;
using Vivoice.Web.Services;

DatabaseInitializationCommand command;
try
{
    command = DatabaseInitializationCommand.Parse(args);
}
catch (ArgumentException)
{
    Console.Error.WriteLine("Invalid database initialization command.");
    Environment.ExitCode = 2;
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDataProtection();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-VIVOICE-CSRF";
    options.Cookie.Name = "vivoice_antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.IPv6Loopback);
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { code = "RATE_LIMITED", message = "Có quá nhiều yêu cầu. Vui lòng thử lại sau." },
            cancellationToken);
    };
    options.AddPolicy("admin-login", context => RateLimitPartition.GetFixedWindowLimiter(
        ClientRateLimitKey.From(context.Connection.RemoteIpAddress),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("public-telemetry", context => RateLimitPartition.GetFixedWindowLimiter(
        ClientRateLimitKey.From(context.Connection.RemoteIpAddress),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
var connectionString = VivoiceConnectionString.Require(builder.Configuration);
builder.Services.AddDbContext<VivoiceDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "vivoice_admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.IsEssential = true;
        options.SlidingExpiration = true;
        options.LoginPath = "/admin.html";
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<PublicContentService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddSingleton<VisitorSessionCookieService>();
builder.Services.AddScoped<BootstrapAdminService>();
builder.Services.AddScoped<IAdminBootstrapper>(serviceProvider => serviceProvider.GetRequiredService<BootstrapAdminService>());
builder.Services.AddScoped<IDatabaseMigrationApplier, EfCoreDatabaseMigrationApplier>();
builder.Services.AddScoped<IContentSeedRunner, ContentSeedRunner>();
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.AddScoped<DatabaseInitializationCommandRunner>();
builder.Services.AddScoped<IDatabaseReadinessService, DatabaseReadinessService>();

var app = builder.Build();

if (command.IsRequested)
{
    try
    {
        await using var initializationScope = app.Services.CreateAsyncScope();
        var runner = initializationScope.ServiceProvider.GetRequiredService<DatabaseInitializationCommandRunner>();
        await runner.RunIfRequestedAsync(command);
    }
    catch (BootstrapAdminConfigurationException exception)
    {
        app.Logger.LogError("{Message}", exception.Message);
        Environment.ExitCode = 1;
    }
    catch
    {
        app.Logger.LogError("Database initialization failed before all requested phases completed.");
        Environment.ExitCode = 1;
    }

    return;
}

app.UseForwardedHeaders();
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "camera=(), geolocation=(), microphone=()";
        context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; base-uri 'self'; connect-src 'self'; font-src 'self' data:; frame-ancestors 'none'; img-src 'self' data: https:; object-src 'none'; script-src 'self' https://unpkg.com; style-src 'self' 'unsafe-inline' https://unpkg.com";
        if (context.Request.IsHttps)
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        return Task.CompletedTask;
    });
    await next();
});
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { code = "SERVER_ERROR", message = "Không thể xử lý yêu cầu lúc này." });
}));
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "alive" }));
app.MapGet("/ready", async (IDatabaseReadinessService readiness, CancellationToken cancellationToken) =>
    await readiness.IsReadyAsync(cancellationToken)
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.Run();

public partial class Program;

public static class ClientRateLimitKey
{
    public static string From(IPAddress? remoteAddress) => remoteAddress?.ToString() ?? "unknown";
}
