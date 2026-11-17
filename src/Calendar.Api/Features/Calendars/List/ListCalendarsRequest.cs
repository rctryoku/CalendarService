using Calendar.Application.Features.Calendars.List;
using FastEndpoints;

namespace Calendar.Api.Features.Calendars.List;

public sealed record ListCalendarsRequest
{
    [QueryParam]
    public string? Owner { get; init; }

    public ListCalendarsQuery ToQuery() => new(Owner);
}
