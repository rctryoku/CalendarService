using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.List;
using Dapper;

namespace Calendar.Data.Features.Events.List;

internal sealed class ListEventsRepository(SqliteConnectionFactory connectionFactory) : IListEventsRepository
{
    public async Task<IReadOnlyList<CalendarEvent>> ListAsync(
        Guid calendarId, DateTime rangeStart, DateTime rangeEnd, CancellationToken cancellationToken)
    {
        var sql = """
            Select
                Id,
                CalendarId,
                Title,
                Description,
                StartDate,
                EndDate,
                RecurrenceFrequency,
                RecurrenceInterval,
                RecurrenceCount
            From
                Events
            Where
                CalendarId = @CalendarId
                And StartDate < @RangeEnd
                And (EndDate > @RangeStart Or RecurrenceFrequency Is Not Null)
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var rows = await connection.QueryAsync<EventRow>(new CommandDefinition(
            sql,
            new { CalendarId = calendarId, RangeStart = rangeStart, RangeEnd = rangeEnd },
            cancellationToken: cancellationToken));
        return [.. rows.Select(row => row.ToCalendarEvent())];
    }
}
