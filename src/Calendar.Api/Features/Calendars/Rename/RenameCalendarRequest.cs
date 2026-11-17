using Calendar.Application.Features.Calendars.Rename;

namespace Calendar.Api.Features.Calendars.Rename;

public sealed record RenameCalendarRequest
{
    public Guid Id { get; init; }

    public string Name { get; init; } = "";

    public RenameCalendarCommand ToCommand() => new(Id, Name);
}
