namespace Calendar.Api.Features.Events.Delete;

public sealed record DeleteEventRequest
{
    public Guid CalendarId { get; init; }

    public Guid Id { get; init; }
}
