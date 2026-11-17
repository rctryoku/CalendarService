using Calendar.Application.Features.Events.List;
using FastEndpoints;

namespace Calendar.Api.Features.Events.List;

public sealed record ListEventsRequest
{
    [QueryParam]
    public DateTimeOffset? From { get; init; }

    [QueryParam]
    public DateTimeOffset? To { get; init; }

    public ListEventsQuery ToQuery() => new(From?.UtcDateTime, To?.UtcDateTime);
}
