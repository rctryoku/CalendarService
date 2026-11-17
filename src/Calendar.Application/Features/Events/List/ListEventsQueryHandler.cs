using Calendar.Application.Features.Calendars;
using Calendar.Core.Features.Calendars.GetById;
using Calendar.Core.Features.Events.List;
using Calendar.Core.Features.Events.Recurrence;
using FastEndpoints;

namespace Calendar.Application.Features.Events.List;

public sealed record ListEventsQuery(Guid CalendarId, DateTime RangeStart, DateTime RangeEnd)
    : ICommand<IReadOnlyList<EventOccurrence>>;

public sealed class ListEventsQueryHandler(
    IListEventsRepository repository,
    IGetCalendarByIdRepository calendars)
    : ICommandHandler<ListEventsQuery, IReadOnlyList<EventOccurrence>>
{
    public async Task<IReadOnlyList<EventOccurrence>> ExecuteAsync(ListEventsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        await calendars.EnsureExistsAsync(query.CalendarId, ct);

        var events = await repository.ListAsync(query.CalendarId, query.RangeStart, query.RangeEnd, ct);

        return
        [
            .. events
                .SelectMany(calendarEvent => EventOccurrences.Between(calendarEvent, query.RangeStart, query.RangeEnd))
                .OrderBy(occurrence => occurrence.StartDate),
        ];
    }
}
