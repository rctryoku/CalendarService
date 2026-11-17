namespace Calendar.Core.Features.Events.Recurrence;

public sealed record EventOccurrence(CalendarEvent CalendarEvent, DateTime StartDate, DateTime EndDate);
