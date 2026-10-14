using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data;
using TourGuide.Api.Providers;
using TourGuide.Api.Services;
using TourGuide.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// MySQL. Chuỗi kết nối KHÔNG viết cứng trong code hay appsettings.json (xem docs/BAO_SETUP.md):
// dùng user-secrets, biến môi trường ConnectionStrings__Default, hoặc bộ biến MYSQL_*.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? ConnectionFromMysqlEnv(builder.Configuration)
    ?? throw new InvalidOperationException(
        "Chưa cấu hình kết nối MySQL. Chạy: dotnet user-secrets set \"ConnectionStrings:Default\" " +
        "\"server=localhost;port=3306;database=tourguide;user=tourguide;password=<MYSQL_PASSWORD>\" --project src/TourGuide.Api");

builder.Services.AddDbContext<IntegrationDbContext>(o =>
    o.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

// Dịch máy (LibreTranslate)
builder.Services.AddHttpClient<ITranslationProvider, LibreTranslateProvider>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["LibreTranslate:BaseUrl"] ?? "http://localhost:5000/");
    c.Timeout = TimeSpan.FromSeconds(90);
});

// Giọng đọc (Edge TTS)
builder.Services.AddSingleton<ITtsProvider, EdgeTtsProvider>();

// Business
builder.Services.AddScoped<TranslationService>();
builder.Services.AddScoped<TtsService>();
builder.Services.AddScoped<JobService>();

// Worker nền
builder.Services.AddHostedService<JobWorker>();

// OpenAPI + Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TourGuide API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Nạp sẵn ngôn ngữ nếu bảng còn trống; chưa migrate thì bỏ qua, không làm app sập.
using (var scope = app.Services.CreateScope())
{
    try
    {
        await LanguageSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<IntegrationDbContext>());
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Bỏ qua seed ngôn ngữ (đã chạy dotnet ef database update chưa?)");
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
