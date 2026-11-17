using Calendar.Core.Features.Calendars.Delete;
using Dapper;

namespace Calendar.Data.Features.Calendars.Delete;

internal sealed class DeleteCalendarRepository(SqliteConnectionFactory connectionFactory) : IDeleteCalendarRepository
{
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = """
            Delete From
                Calendars
            Where
                Id = @Id
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return affectedRows > 0;
    }
}
