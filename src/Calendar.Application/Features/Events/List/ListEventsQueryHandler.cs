using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.List;
using FastEndpoints;

namespace Calendar.Application.Features.Events.List;

public sealed record ListEventsQuery(DateTime? RangeStart, DateTime? RangeEnd) : ICommand<IReadOnlyList<CalendarEvent>>;

public sealed class ListEventsQueryHandler(IListEventsRepository repository)
    : ICommandHandler<ListEventsQuery, IReadOnlyList<CalendarEvent>>
{
    public Task<IReadOnlyList<CalendarEvent>> ExecuteAsync(ListEventsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        return repository.ListAsync(query.RangeStart, query.RangeEnd, ct);
    }
}
