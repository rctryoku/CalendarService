using Calendar.Application.Features.Events.Create;

namespace Calendar.Api.Features.Events.Create;

public sealed record CreateEventRequest : IEventDetails
{
    public Guid CalendarId { get; init; }

    public string Title { get; init; } = "";

    public string? Description { get; init; }

    public DateTimeOffset StartDate { get; init; }

    public DateTimeOffset EndDate { get; init; }

    public CreateEventCommand ToCommand() =>
        new(CalendarId, Title, Description, StartDate.UtcDateTime, EndDate.UtcDateTime);
}
