namespace Calendar.Api.Features.Calendars.Delete;

public sealed record DeleteCalendarRequest
{
    public Guid Id { get; init; }
}
