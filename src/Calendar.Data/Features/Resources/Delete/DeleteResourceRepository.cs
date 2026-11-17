using Calendar.Core.Features.Resources.Delete;
using Dapper;

namespace Calendar.Data.Features.Resources.Delete;

internal sealed class DeleteResourceRepository(SqliteConnectionFactory connectionFactory) : IDeleteResourceRepository
{
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = """
            Delete From
                Resources
            Where
                Id = @Id
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return affectedRows > 0;
    }
}
