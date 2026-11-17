using Calendar.Core.Features.Calendars;
using Calendar.Core.Features.Calendars.GetById;
using Dapper;

namespace Calendar.Data.Features.Calendars.GetById;

internal sealed class GetCalendarByIdRepository(SqliteConnectionFactory connectionFactory) : IGetCalendarByIdRepository
{
    public async Task<UserCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = """
            Select
                Id,
                Name,
                Owner
            From
                Calendars
            Where
                Id = @Id
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<UserCalendar>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }
}
