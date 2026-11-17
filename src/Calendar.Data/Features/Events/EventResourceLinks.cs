using System.Data;
using Calendar.Core.Features.Events;
using Calendar.Data.TypeHandlers;
using Dapper;

namespace Calendar.Data.Features.Events;

internal static class EventResourceLinks
{
    public static async Task ReplaceAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        Guid eventId,
        IReadOnlyList<Guid> resourceIds,
        CancellationToken cancellationToken)
    {
        var deleteSql = """
            Delete From
                EventResources
            Where
                EventId = @EventId
            """;

        var insertSql = """
            Insert Into EventResources
            (
                EventId,
                ResourceId
            )
            Values
            (
                @EventId,
                @ResourceId
            )
            """;

        await connection.ExecuteAsync(new CommandDefinition(
            deleteSql, new { EventId = eventId }, transaction, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            insertSql,
            resourceIds.Select(resourceId => new { EventId = eventId, ResourceId = resourceId }),
            transaction,
            cancellationToken: cancellationToken));
    }

    public static async Task<IReadOnlyList<CalendarEvent>> WithResourcesAsync(
        IDbConnection connection, IEnumerable<EventRow> rows, CancellationToken cancellationToken)
    {
        var eventRows = rows.ToList();
        var resourceIds = await LoadAsync(connection, eventRows.Select(row => row.Id), cancellationToken);
        return [.. eventRows.Select(row => row.ToCalendarEvent([.. resourceIds[row.Id]]))];
    }

    private static async Task<ILookup<Guid, Guid>> LoadAsync(
        IDbConnection connection, IEnumerable<Guid> eventIds, CancellationToken cancellationToken)
    {
        var sql = """
            Select
                EventId,
                ResourceId
            From
                EventResources
            Where
                EventId In @EventIds
            Order By
                ResourceId
            """;

        var links = await connection.QueryAsync<EventResourceLinkRow>(new CommandDefinition(
            sql, new { EventIds = GuidList.AsText(eventIds) }, cancellationToken: cancellationToken));
        return links.ToLookup(link => link.EventId, link => link.ResourceId);
    }

    private sealed class EventResourceLinkRow
    {
        public Guid EventId { get; set; }

        public Guid ResourceId { get; set; }
    }
}
