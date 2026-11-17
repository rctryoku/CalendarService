using Calendar.Core.Features.Calendars;
using Calendar.Core.Features.Calendars.List;
using FastEndpoints;

namespace Calendar.Application.Features.Calendars.List;

public sealed record ListCalendarsQuery(string? Owner) : ICommand<IReadOnlyList<UserCalendar>>;

public sealed class ListCalendarsQueryHandler(IListCalendarsRepository repository)
    : ICommandHandler<ListCalendarsQuery, IReadOnlyList<UserCalendar>>
{
    public Task<IReadOnlyList<UserCalendar>> ExecuteAsync(ListCalendarsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        return repository.ListAsync(query.Owner, ct);
    }
}
