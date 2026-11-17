using Calendar.Application.Features.Events.List;
using FastEndpoints;

namespace Calendar.Api.Features.Events.List;

public sealed record ListEventsRequest
{
    public Guid CalendarId { get; init; }

    [QueryParam]
    public DateTimeOffset? From { get; init; }

    [QueryParam]
    public DateTimeOffset? To { get; init; }

    public ListEventsQuery ToQuery() => new(CalendarId, From?.UtcDateTime, To?.UtcDateTime);
}
