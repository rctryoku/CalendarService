using Microsoft.Data.Sqlite;

namespace Calendar.Data;

internal sealed class SqliteConnectionFactory(string connectionString)
{
    // SQLite ignores foreign keys (and so "On Delete Cascade") unless each connection turns them on.
    private readonly string _connectionString =
        new SqliteConnectionStringBuilder(connectionString) { ForeignKeys = true }.ToString();

    public async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
