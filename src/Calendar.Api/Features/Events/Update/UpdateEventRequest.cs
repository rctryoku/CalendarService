using Calendar.Application.Features.Events.Update;

namespace Calendar.Api.Features.Events.Update;

public sealed record UpdateEventRequest : IEventDetails
{
    public Guid CalendarId { get; init; }

    public Guid Id { get; init; }

    public string Title { get; init; } = "";

    public string? Description { get; init; }

    public DateTimeOffset StartDate { get; init; }

    public DateTimeOffset EndDate { get; init; }

    public RecurrenceRequest? Recurrence { get; init; }

    public IReadOnlyList<Guid> ResourceIds { get; init; } = [];

    public UpdateEventCommand ToCommand() => new(
        CalendarId, Id, Title, Description, StartDate.UtcDateTime, EndDate.UtcDateTime, Recurrence?.ToRule(), ResourceIds);
}
