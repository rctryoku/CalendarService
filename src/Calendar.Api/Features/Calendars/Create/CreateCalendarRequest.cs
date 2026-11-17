using Calendar.Application.Features.Calendars.Create;

namespace Calendar.Api.Features.Calendars.Create;

public sealed record CreateCalendarRequest
{
    public string Name { get; init; } = "";

    public string Owner { get; init; } = "";

    public CreateCalendarCommand ToCommand() => new(Name, Owner);
}
