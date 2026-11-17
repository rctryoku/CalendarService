using Calendar.Core.Features.Calendars;
using Calendar.Core.Features.Calendars.List;
using Dapper;

namespace Calendar.Data.Features.Calendars.List;

internal sealed class ListCalendarsRepository(SqliteConnectionFactory connectionFactory) : IListCalendarsRepository
{
    public async Task<IReadOnlyList<UserCalendar>> ListAsync(string? owner, CancellationToken cancellationToken)
    {
        var sql = """
            Select
                Id,
                Name,
                Owner
            From
                Calendars
            Where
                @Owner Is Null Or Owner = @Owner
            Order By
                Name
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var calendars = await connection.QueryAsync<UserCalendar>(
            new CommandDefinition(sql, new { Owner = owner }, cancellationToken: cancellationToken));
        return [.. calendars];
    }
}
