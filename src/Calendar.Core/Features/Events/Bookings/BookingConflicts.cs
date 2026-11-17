using Calendar.Core.Features.Events.Recurrence;

namespace Calendar.Core.Features.Events.Bookings;

public static class BookingConflicts
{
    /// <summary>How far ahead a series without a count is checked for conflicts.</summary>
    public static readonly TimeSpan OpenEndedHorizon = TimeSpan.FromDays(366);

    public static (DateTime Start, DateTime End) CheckWindow(CalendarEvent calendarEvent)
    {
        ArgumentNullException.ThrowIfNull(calendarEvent);

        return (calendarEvent.StartDate, SeriesEnd(calendarEvent));
    }

    /// <summary>The first occurrence of another event that books one of the same resources at an overlapping time.</summary>
    public static BookingConflict? FindFirst(CalendarEvent calendarEvent, IEnumerable<CalendarEvent> otherEvents)
    {
        var (start, end) = CheckWindow(calendarEvent);
        var competitors = otherEvents
            .Where(other => other.Id != calendarEvent.Id && SharedResource(calendarEvent, other) is not null)
            .ToList();

        return EventOccurrences.Between(calendarEvent, start, end)
            .SelectMany(occurrence => ClashesWith(occurrence, competitors))
            .FirstOrDefault();
    }

    private static IEnumerable<BookingConflict> ClashesWith(EventOccurrence occurrence, List<CalendarEvent> competitors) =>
        competitors.SelectMany(competitor => EventOccurrences
            .Between(competitor, occurrence.StartDate, occurrence.EndDate)
            .Select(clash => new BookingConflict(
                competitor, SharedResource(occurrence.CalendarEvent, competitor)!.Value, clash.StartDate)));

    private static Guid? SharedResource(CalendarEvent calendarEvent, CalendarEvent other) =>
        calendarEvent.ResourceIds.Intersect(other.ResourceIds).Select(id => (Guid?)id).FirstOrDefault();

    private static DateTime SeriesEnd(CalendarEvent calendarEvent) => calendarEvent.Recurrence switch
    {
        null => calendarEvent.EndDate,
        { Count: { } count } rule => calendarEvent.EndDate + TimeSpan.FromTicks(rule.Step.Ticks * (count - 1)),
        _ => calendarEvent.StartDate + OpenEndedHorizon,
    };
}
