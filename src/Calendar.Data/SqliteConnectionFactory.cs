using Microsoft.Data.Sqlite;

namespace Calendar.Data;

internal sealed class SqliteConnectionFactory(string connectionString)
{
    public async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
