using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Recurrence;

namespace Calendar.Data.Features.Events;

internal sealed class EventRow
{
    public Guid Id { get; set; }

    public Guid CalendarId { get; set; }

    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int? RecurrenceFrequency { get; set; }

    public int? RecurrenceInterval { get; set; }

    public int? RecurrenceCount { get; set; }

    public static EventRow From(CalendarEvent calendarEvent) => new()
    {
        Id = calendarEvent.Id,
        CalendarId = calendarEvent.CalendarId,
        Title = calendarEvent.Title,
        Description = calendarEvent.Description,
        StartDate = calendarEvent.StartDate,
        EndDate = calendarEvent.EndDate,
        RecurrenceFrequency = (int?)calendarEvent.Recurrence?.Frequency,
        RecurrenceInterval = calendarEvent.Recurrence?.Interval,
        RecurrenceCount = calendarEvent.Recurrence?.Count,
    };

    public CalendarEvent ToCalendarEvent(IReadOnlyList<Guid> resourceIds) =>
        new(Id, CalendarId, Title, Description, StartDate, EndDate, ToRecurrenceRule(), resourceIds);

    private RecurrenceRule? ToRecurrenceRule()
    {
        if (RecurrenceFrequency is null)
            return null;

        return new RecurrenceRule(
            (RecurrenceFrequency)RecurrenceFrequency.Value,
            RecurrenceInterval ?? 1,
            RecurrenceCount);
    }
}
