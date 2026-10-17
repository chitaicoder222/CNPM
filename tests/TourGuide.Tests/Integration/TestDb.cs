using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TourGuide.Api.Data;

namespace TourGuide.Tests.Integration;

public sealed class TestDb : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    public IntegrationDbContext Context { get; }

    public TestDb()
    {
        _connection.Open();
        Context = new IntegrationDbContext(new DbContextOptionsBuilder<IntegrationDbContext>()
            .UseSqlite(_connection)
            .Options);
        Context.Database.EnsureCreated();
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
