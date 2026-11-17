using Calendar.Application.Features.Events.Create;

namespace Calendar.Api.Features.Events.Create;

public sealed record CreateEventRequest : IEventDetails
{
    public string Title { get; init; } = "";

    public string? Description { get; init; }

    public DateTimeOffset StartDate { get; init; }

    public DateTimeOffset EndDate { get; init; }

    public CreateEventCommand ToCommand() =>
        new(Title, Description, StartDate.UtcDateTime, EndDate.UtcDateTime);
}
