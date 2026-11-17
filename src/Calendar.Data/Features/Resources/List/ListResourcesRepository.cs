using Calendar.Core.Features.Resources;
using Calendar.Core.Features.Resources.List;
using Dapper;

namespace Calendar.Data.Features.Resources.List;

internal sealed class ListResourcesRepository(SqliteConnectionFactory connectionFactory) : IListResourcesRepository
{
    public async Task<IReadOnlyList<Resource>> ListAsync(CancellationToken cancellationToken)
    {
        var sql = """
            Select
                Id,
                Name
            From
                Resources
            Order By
                Name
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var resources = await connection.QueryAsync<Resource>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        return [.. resources];
    }
}
