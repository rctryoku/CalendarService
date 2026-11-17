using Calendar.Core.Features.Events.Recurrence;

namespace Calendar.Api.Features.Events.List;

public sealed record EventOccurrenceResponse(
    Guid EventId,
    Guid CalendarId,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsRecurring)
{
    public static EventOccurrenceResponse From(EventOccurrence occurrence) => new(
        occurrence.CalendarEvent.Id,
        occurrence.CalendarEvent.CalendarId,
        occurrence.CalendarEvent.Title,
        occurrence.CalendarEvent.Description,
        occurrence.StartDate,
        occurrence.EndDate,
        occurrence.CalendarEvent.Recurrence is not null);
}
