// Purpose: Configures the VIVOICE web host, dependency injection, authentication and HTTP pipeline.
// Responsibility: Keeps public static UI, protected admin APIs and SQL Server access in one modular-monolith host.
// Important: Secrets and the SQL connection string come only from User Secrets or environment variables.
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddControllers();
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
app.MapGet("/health", () => Results.Ok(new { status = "alive" }));
app.MapGet("/ready", async (IDatabaseReadinessService readiness, CancellationToken cancellationToken) =>
    await readiness.IsReadyAsync(cancellationToken)
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.Run();

public partial class Program;
