using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Update;
using Dapper;

namespace Calendar.Data.Features.Events.Update;

internal sealed class UpdateEventRepository(SqliteConnectionFactory connectionFactory) : IUpdateEventRepository
{
    public async Task<bool> UpdateAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken)
    {
        var sql = """
            Update
                Events
            Set
                Title = @Title,
                Description = @Description,
                StartDate = @StartDate,
                EndDate = @EndDate
            Where
                Id = @Id
                And CalendarId = @CalendarId
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(sql, calendarEvent, cancellationToken: cancellationToken));
        return affectedRows > 0;
    }
}
