using Calendar.Core.Features.Events;

namespace Calendar.Api.Features.Events;

public sealed record EventResponse(Guid Id, string Title, string? Description, DateTime StartDate, DateTime EndDate)
{
    public static EventResponse From(CalendarEvent calendarEvent) => new(
        calendarEvent.Id,
        calendarEvent.Title,
        calendarEvent.Description,
        calendarEvent.StartDate,
        calendarEvent.EndDate);
}
