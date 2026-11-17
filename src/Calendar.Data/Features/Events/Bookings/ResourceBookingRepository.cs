using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Bookings;
using Calendar.Core.Features.Resources;
using Dapper;

namespace Calendar.Data.Features.Events.Bookings;

internal sealed class ResourceBookingRepository(SqliteConnectionFactory connectionFactory) : IResourceBookingRepository
{
    public async Task<IReadOnlyList<Resource>> GetResourcesAsync(
        IReadOnlyCollection<Guid> resourceIds, CancellationToken cancellationToken)
    {
        var sql = """
            Select
                Id,
                Name
            From
                Resources
            Where
                Id In @ResourceIds
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var resources = await connection.QueryAsync<Resource>(new CommandDefinition(
            sql, new { ResourceIds = AsText(resourceIds) }, cancellationToken: cancellationToken));
        return [.. resources];
    }

    public async Task<IReadOnlyList<CalendarEvent>> ListBookingsAsync(
        IReadOnlyCollection<Guid> resourceIds,
        Guid excludeEventId,
        DateTime rangeStart,
        DateTime rangeEnd,
        CancellationToken cancellationToken)
    {
        var sql = """
            Select Distinct
                Events.Id,
                Events.CalendarId,
                Events.Title,
                Events.Description,
                Events.StartDate,
                Events.EndDate,
                Events.RecurrenceFrequency,
                Events.RecurrenceInterval,
                Events.RecurrenceCount
            From
                Events
                Inner Join EventResources On EventResources.EventId = Events.Id
            Where
                EventResources.ResourceId In @ResourceIds
                And Events.Id <> @ExcludeEventId
                And Events.StartDate < @RangeEnd
                And (Events.EndDate > @RangeStart Or Events.RecurrenceFrequency Is Not Null)
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        var rows = await connection.QueryAsync<EventRow>(new CommandDefinition(
            sql,
            new
            {
                ResourceIds = AsText(resourceIds),
                ExcludeEventId = excludeEventId,
                RangeStart = rangeStart,
                RangeEnd = rangeEnd,
            },
            cancellationToken: cancellationToken));
        return await EventResourceLinks.WithResourcesAsync(connection, rows, cancellationToken);
    }

    // Dapper's list expansion skips type handlers, so the ids are passed as the text they are stored as.
    private static IEnumerable<string> AsText(IEnumerable<Guid> ids) => ids.Select(id => id.ToString());
}
