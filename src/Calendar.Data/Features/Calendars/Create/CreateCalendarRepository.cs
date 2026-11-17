using Calendar.Core.Features.Calendars;
using Calendar.Core.Features.Calendars.Create;
using Dapper;

namespace Calendar.Data.Features.Calendars.Create;

internal sealed class CreateCalendarRepository(SqliteConnectionFactory connectionFactory) : ICreateCalendarRepository
{
    public async Task CreateAsync(UserCalendar calendar, CancellationToken cancellationToken)
    {
        var sql = """
            Insert Into Calendars
            (
                Id,
                Name,
                Owner
            )
            Values
            (
                @Id,
                @Name,
                @Owner
            )
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, calendar, cancellationToken: cancellationToken));
    }
}
