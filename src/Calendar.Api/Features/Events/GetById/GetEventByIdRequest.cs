namespace Calendar.Api.Features.Events.GetById;

public sealed record GetEventByIdRequest
{
    public Guid CalendarId { get; init; }

    public Guid Id { get; init; }
}
