using Calendar.Application.Features.Events.Delete;

namespace Calendar.Api.Features.Events.Delete;

public sealed record DeleteEventRequest
{
    public Guid CalendarId { get; init; }

    public Guid Id { get; init; }

    public DeleteEventCommand ToCommand() => new(CalendarId, Id);
}
