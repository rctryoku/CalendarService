using Calendar.Core.Features.Events.Delete;
using Dapper;

namespace Calendar.Data.Features.Events.Delete;

internal sealed class DeleteEventRepository(SqliteConnectionFactory connectionFactory) : IDeleteEventRepository
{
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = """
            Delete From
                Events
            Where
                Id = @Id
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return affectedRows > 0;
    }
}
