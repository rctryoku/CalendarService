using Calendar.Application.Features.Calendars;
using Calendar.Core.Features.Calendars.GetById;
using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.List;
using FastEndpoints;

namespace Calendar.Application.Features.Events.List;

public sealed record ListEventsQuery(Guid CalendarId, DateTime? RangeStart, DateTime? RangeEnd)
    : ICommand<IReadOnlyList<CalendarEvent>>;

public sealed class ListEventsQueryHandler(
    IListEventsRepository repository,
    IGetCalendarByIdRepository calendars)
    : ICommandHandler<ListEventsQuery, IReadOnlyList<CalendarEvent>>
{
    public async Task<IReadOnlyList<CalendarEvent>> ExecuteAsync(ListEventsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        await calendars.EnsureExistsAsync(query.CalendarId, ct);

        return await repository.ListAsync(query.CalendarId, query.RangeStart, query.RangeEnd, ct);
    }
}
