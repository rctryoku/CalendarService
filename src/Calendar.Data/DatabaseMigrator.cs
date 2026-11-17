using System.Data.Common;
using Dapper;

namespace Calendar.Data;

/// <summary>
/// Runs the plain SQL scripts in <c>Migrations/</c> in file name order, each exactly once.
/// Applied scripts are recorded in <c>SchemaVersions</c>.
/// </summary>
internal sealed class DatabaseMigrator(SqliteConnectionFactory connectionFactory)
{
    private const string ResourcePrefix = "Migrations/";

    private const string CreateSchemaVersionsTable = """
        Create Table If Not Exists SchemaVersions
        (
            ScriptName Text Not Null Primary Key,
            AppliedAt  Text Not Null
        );
        """;

    private const string SelectAppliedScripts = """
        Select
            ScriptName
        From
            SchemaVersions
        """;

    private const string InsertAppliedScript = """
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

    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(CreateSchemaVersionsTable, cancellationToken: cancellationToken));

        var appliedScripts = await connection.QueryAsync<string>(
            new CommandDefinition(SelectAppliedScripts, cancellationToken: cancellationToken));

        foreach (var scriptName in PendingScriptNames(appliedScripts.ToHashSet()))
            await ApplyAsync(connection, scriptName, cancellationToken);
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
        var script = await ReadScriptAsync(scriptName, cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(script, transaction: transaction, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            InsertAppliedScript,
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
