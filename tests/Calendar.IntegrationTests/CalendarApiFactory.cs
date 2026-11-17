using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Calendar.IntegrationTests;

public sealed class CalendarApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString =
        $"Data Source=file:test-{Guid.NewGuid()}?mode=memory&cache=shared";

    // An in-memory SQLite database only lives while a connection to it is open.
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
