using Calendar.Application.Features.Calendars.Delete;

namespace Calendar.Api.Features.Calendars.Delete;

public sealed record DeleteCalendarRequest
{
    public Guid Id { get; init; }

    public DeleteCalendarCommand ToCommand() => new(Id);
}
