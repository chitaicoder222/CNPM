// Purpose: Configures the VIVOICE web host, dependency injection, authentication and HTTP pipeline.
// Responsibility: Keeps public static UI, protected admin APIs and SQL Server access in one modular-monolith host.
// Important: Secrets and the SQL connection string come only from User Secrets or environment variables.
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Vivoice.Web.Data;
using Vivoice.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<VivoiceDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Vivoice");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("ConnectionStrings:Vivoice is required. Configure User Secrets or environment variables.");
    }

    options.UseSqlServer(connectionString);
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "vivoice_admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
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
builder.Services.AddScoped<BootstrapAdminService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { code = "SERVER_ERROR", message = "Không thể xử lý yêu cầu lúc này." });
}));
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VivoiceDbContext>();
    await db.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<BootstrapAdminService>().SeedAsync();
    await DatabaseSeeder.SeedAsync(db);
}

app.Run();

public partial class Program;
