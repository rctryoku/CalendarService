using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.List;
using Dapper;

namespace Calendar.Data.Features.Events.List;

internal sealed class ListEventsRepository(SqliteConnectionFactory connectionFactory) : IListEventsRepository
{
    public async Task<IReadOnlyList<CalendarEvent>> ListAsync(
        DateTime? rangeStart, DateTime? rangeEnd, CancellationToken cancellationToken)
    {
        var sql = """
            Select
                Id,
                Title,
                Description,
                StartDate,
                EndDate
            From
                Events
            Where
                (@RangeStart Is Null Or EndDate > @RangeStart)
                And (@RangeEnd Is Null Or StartDate < @RangeEnd)
            Order By
                StartDate
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var events = await connection.QueryAsync<CalendarEvent>(new CommandDefinition(
            sql, new { RangeStart = rangeStart, RangeEnd = rangeEnd }, cancellationToken: cancellationToken));
        return [.. events];
    }
}
