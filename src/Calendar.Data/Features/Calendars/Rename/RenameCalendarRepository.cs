using Calendar.Core.Features.Calendars;
using Calendar.Core.Features.Calendars.Rename;
using Dapper;

namespace Calendar.Data.Features.Calendars.Rename;

internal sealed class RenameCalendarRepository(SqliteConnectionFactory connectionFactory) : IRenameCalendarRepository
{
    public async Task<UserCalendar?> RenameAsync(Guid id, string name, CancellationToken cancellationToken)
    {
        var sql = """
            Update
                Calendars
            Set
                Name = @Name
            Where
                Id = @Id
            Returning
                Id,
                Name,
                Owner
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<UserCalendar>(
            new CommandDefinition(sql, new { Id = id, Name = name }, cancellationToken: cancellationToken));
    }
}
