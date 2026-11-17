using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.GetById;
using Dapper;

namespace Calendar.Data.Features.Events.GetById;

internal sealed class GetEventByIdRepository(SqliteConnectionFactory connectionFactory) : IGetEventByIdRepository
{
    public async Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
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
                Id = @Id
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<CalendarEvent>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }
}
