using Calendar.Core.Features.Resources;
using Calendar.Core.Features.Resources.GetById;
using Dapper;

namespace Calendar.Data.Features.Resources.GetById;

internal sealed class GetResourceByIdRepository(SqliteConnectionFactory connectionFactory) : IGetResourceByIdRepository
{
    public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = """
            Select
                Id,
                Name
            From
                Resources
            Where
                Id = @Id
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Resource>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }
}
