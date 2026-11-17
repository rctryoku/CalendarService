using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Create;
using Dapper;

namespace Calendar.Data.Features.Events.Create;

internal sealed class CreateEventRepository(SqliteConnectionFactory connectionFactory) : ICreateEventRepository
{
    public async Task CreateAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken)
    {
        var sql = """
            Insert Into Events
            (
                Id,
                CalendarId,
                Title,
                Description,
                StartDate,
                EndDate,
                RecurrenceFrequency,
                RecurrenceInterval,
                RecurrenceCount
            )
            Values
            (
                @Id,
                @CalendarId,
                @Title,
                @Description,
                @StartDate,
                @EndDate,
                @RecurrenceFrequency,
                @RecurrenceInterval,
                @RecurrenceCount
            )
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, EventRow.From(calendarEvent), cancellationToken: cancellationToken));
    }
}
