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
                Title,
                Description,
                StartDate,
                EndDate
            )
            Values
            (
                @Id,
                @Title,
                @Description,
                @StartDate,
                @EndDate
            )
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, calendarEvent, cancellationToken: cancellationToken));
    }
}
