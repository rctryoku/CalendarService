using Calendar.Application.Features.Events.GetById;

namespace Calendar.Api.Features.Events.GetById;

public sealed record GetEventByIdRequest
{
    public Guid CalendarId { get; init; }

    public Guid Id { get; init; }

    public GetEventByIdQuery ToQuery() => new(CalendarId, Id);
}
