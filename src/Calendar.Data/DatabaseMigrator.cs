using System.Data.Common;
using Dapper;

namespace Calendar.Data;

internal sealed class DatabaseMigrator(SqliteConnectionFactory connectionFactory)
{
    private const string ResourcePrefix = "Migrations/";

    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var appliedScripts = await AppliedScriptNamesAsync(connection, cancellationToken);

        foreach (var scriptName in PendingScriptNames(appliedScripts))
            await ApplyAsync(connection, scriptName, cancellationToken);
    }

    private static async Task<HashSet<string>> AppliedScriptNamesAsync(
        DbConnection connection, CancellationToken cancellationToken)
    {
        var createSql = """
            Create Table If Not Exists SchemaVersions
            (
                ScriptName Text Not Null Primary Key,
                AppliedAt  Text Not Null
            );
            """;

        var selectSql = """
            Select
                ScriptName
            From
                SchemaVersions
            """;

        await connection.ExecuteAsync(new CommandDefinition(createSql, cancellationToken: cancellationToken));
        var scriptNames = await connection.QueryAsync<string>(
            new CommandDefinition(selectSql, cancellationToken: cancellationToken));
        return scriptNames.ToHashSet();
    }

    private static IEnumerable<string> PendingScriptNames(HashSet<string> appliedScripts) =>
        typeof(DatabaseMigrator).Assembly
            .GetManifestResourceNames()
            .Where(resourceName => resourceName.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            .Select(resourceName => resourceName[ResourcePrefix.Length..])
            .Where(scriptName => !appliedScripts.Contains(scriptName))
            .Order(StringComparer.Ordinal);

    private static async Task ApplyAsync(DbConnection connection, string scriptName, CancellationToken cancellationToken)
    {
        var sql = """
            Insert Into SchemaVersions
            (
                ScriptName,
                AppliedAt
            )
            Values
            (
                @ScriptName,
                @AppliedAt
            )
            """;

        var script = await ReadScriptAsync(scriptName, cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(script, transaction: transaction, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { ScriptName = scriptName, AppliedAt = DateTime.UtcNow },
            transaction,
            cancellationToken: cancellationToken));
        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task<string> ReadScriptAsync(string scriptName, CancellationToken cancellationToken)
    {
        await using var stream = typeof(DatabaseMigrator).Assembly.GetManifestResourceStream(ResourcePrefix + scriptName)
            ?? throw new InvalidOperationException($"Migration script '{scriptName}' was not found.");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
