using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.GetById;
using Dapper;

namespace Calendar.Data.Features.Events.GetById;

internal sealed class GetEventByIdRepository(SqliteConnectionFactory connectionFactory) : IGetEventByIdRepository
{
    public async Task<CalendarEvent?> GetByIdAsync(Guid calendarId, Guid id, CancellationToken cancellationToken)
    {
        var sql = """
            Select
                Id,
                CalendarId,
                Title,
                Description,
                StartDate,
                EndDate
            From
                Events
            Where
                Id = @Id
                And CalendarId = @CalendarId
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<CalendarEvent>(new CommandDefinition(
            sql, new { Id = id, CalendarId = calendarId }, cancellationToken: cancellationToken));
    }
}
