using Calendar.Core.Features.Events;

namespace Calendar.Api.Features.Events;

public sealed record EventResponse(
    Guid Id,
    Guid CalendarId,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    RecurrenceResponse? Recurrence)
{
    public static EventResponse From(CalendarEvent calendarEvent) => new(
        calendarEvent.Id,
        calendarEvent.CalendarId,
        calendarEvent.Title,
        calendarEvent.Description,
        calendarEvent.StartDate,
        calendarEvent.EndDate,
        RecurrenceResponse.From(calendarEvent.Recurrence));
}
