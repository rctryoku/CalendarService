using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Calendar.IntegrationTests;

/// <summary>
/// Boots the real API in memory against a private in-memory SQLite database.
/// The database lives as long as <see cref="_keepAlive"/> is open, so each
/// factory instance (one per test) gets a fresh, isolated database.
/// </summary>
public sealed class CalendarApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString =
        $"Data Source=file:test-{Guid.NewGuid()}?mode=memory&cache=shared";

    private readonly SqliteConnection _keepAlive;

    public CalendarApiFactory()
    {
        _keepAlive = new SqliteConnection(_connectionString);
        _keepAlive.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Calendar", _connectionString);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _keepAlive.Dispose();
    }
}
