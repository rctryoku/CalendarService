using Calendar.Core.Features.Events.Recurrence;

namespace Calendar.Core.Features.Events.Bookings;

public static class BookingConflicts
{
    public static readonly TimeSpan EndlessSeriesHorizon = TimeSpan.FromDays(366);

    public static (DateTime Start, DateTime End) CheckWindow(CalendarEvent calendarEvent)
    {
        ArgumentNullException.ThrowIfNull(calendarEvent);

        return (calendarEvent.StartDate, SeriesEnd(calendarEvent));
    }

    public static BookingConflict? FindFirst(CalendarEvent calendarEvent, IEnumerable<CalendarEvent> otherEvents)
    {
        var (start, end) = CheckWindow(calendarEvent);
        var competitors = otherEvents
            .Where(other => other.Id != calendarEvent.Id && SharesResource(calendarEvent, other))
            .ToList();

        return EventOccurrences.Between(calendarEvent, start, end)
            .SelectMany(occurrence => competitors.SelectMany(competitor => Clashes(occurrence, competitor)))
            .FirstOrDefault();
    }

    private static IEnumerable<BookingConflict> Clashes(EventOccurrence occurrence, CalendarEvent competitor) =>
        EventOccurrences.Between(competitor, occurrence.StartDate, occurrence.EndDate)
            .Select(clash => new BookingConflict(
                competitor, SharedResource(occurrence.CalendarEvent, competitor), clash.StartDate));

    private static bool SharesResource(CalendarEvent calendarEvent, CalendarEvent other) =>
        calendarEvent.ResourceIds.Intersect(other.ResourceIds).Any();

    private static Guid SharedResource(CalendarEvent calendarEvent, CalendarEvent other) =>
        calendarEvent.ResourceIds.Intersect(other.ResourceIds).First();

    private static DateTime SeriesEnd(CalendarEvent calendarEvent) => calendarEvent.Recurrence switch
    {
        null => calendarEvent.EndDate,
        { Count: { } count } rule => calendarEvent.EndDate + TimeSpan.FromTicks(rule.Step.Ticks * (count - 1)),
        _ => calendarEvent.StartDate + EndlessSeriesHorizon,
    };
}
