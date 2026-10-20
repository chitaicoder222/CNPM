using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TourGuide.Api.Data;
using TourGuide.Api.Providers;
using TourGuide.Api.Services;
using TourGuide.Api.Workers;
using TourGuide.Api.Repositories;
using Microsoft.Extensions.FileProviders;
using System.IO;
var builder = WebApplication.CreateBuilder(args);

// Keep local/container diagnostics on stdout; Windows EventLog is not writable
// in non-elevated developer shells and can mask the real database exception.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

// JWT Authentication (read secret/issuer from configuration)
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException(
        "Chưa cấu hình Jwt:Secret. Dùng dotnet user-secrets hoặc biến môi trường; không ghi secret vào source.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "TourGuide",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// MySQL connection (from config/environment)
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? ConnectionFromMysqlEnv(builder.Configuration)
    ?? throw new InvalidOperationException(
        "Chưa cấu hình kết nối MySQL. Sử dụng dotnet user-secrets hoặc biến môi trường.");

builder.Services.AddDbContext<IntegrationDbContext>(o =>
    o.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

// Providers
builder.Services.AddHttpClient<ITranslationProvider, LibreTranslateProvider>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["LibreTranslate:BaseUrl"] ?? "http://localhost:5000/");
    c.Timeout = TimeSpan.FromSeconds(90);
});

builder.Services.AddSingleton<ITtsProvider, EdgeTtsProvider>();

// Business services
builder.Services.AddScoped<TranslationService>();
builder.Services.AddScoped<TtsService>();
builder.Services.AddScoped<JobService>();

// Repositories (minimal abstraction)
builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IPlaceRepository, PlaceRepository>();
builder.Services.AddScoped<PlaceService>();
builder.Services.AddScoped<LanguageService>();

// Background worker
builder.Services.AddHostedService<JobWorker>();

// Swagger / OpenAPI (dev only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
// Apply pending migrations on startup
await using var migrateScope = app.Services.CreateAsyncScope();
var dbContext = migrateScope.ServiceProvider.GetRequiredService<IntegrationDbContext>();
await dbContext.Database.MigrateAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TourGuide API v1"));
}

// app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
var storageRoot = Path.GetFullPath(builder.Configuration["Storage:Root"] ?? "storage", app.Environment.ContentRootPath);
Directory.CreateDirectory(storageRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storageRoot),
    RequestPath = "/storage"
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Seed languages if DB empty (ignore migration errors)
using (var scope = app.Services.CreateScope())
{
    try
    {
        await LanguageSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<IntegrationDbContext>());
        await NarrationCatalogSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<IntegrationDbContext>());
        await UserSeeder.SeedConfiguredAsync(scope.ServiceProvider.GetRequiredService<IntegrationDbContext>(), scope.ServiceProvider.GetRequiredService<IConfiguration>());
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Bỏ qua seed ngôn ngữ (có thể chưa chạy migration)");
    }
}

app.Run();

static string? ConnectionFromMysqlEnv(IConfiguration c)
{
    var password = c["MYSQL_PASSWORD"];
    if (string.IsNullOrEmpty(password)) return null;
    var host = c["MYSQL_HOST"] ?? "localhost";
    var port = c["MYSQL_PORT"] ?? "3306";
    var database = c["MYSQL_DATABASE"] ?? "tourguide";
    var user = c["MYSQL_USER"] ?? "tourguide";
    return $"server={host};port={port};database={database};user={user};password={password}";
}
