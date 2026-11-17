namespace Calendar.Core.Features.Events.Recurrence;

public static class EventOccurrences
{
    public static IEnumerable<EventOccurrence> Between(CalendarEvent calendarEvent, DateTime rangeStart, DateTime rangeEnd)
    {
        ArgumentNullException.ThrowIfNull(calendarEvent);

        return calendarEvent.Recurrence is null
            ? Single(calendarEvent, rangeStart, rangeEnd)
            : Repeated(calendarEvent, calendarEvent.Recurrence, rangeStart, rangeEnd);
    }

    private static IEnumerable<EventOccurrence> Single(CalendarEvent calendarEvent, DateTime rangeStart, DateTime rangeEnd)
    {
        if (calendarEvent.StartDate < rangeEnd && rangeStart < calendarEvent.EndDate)
            yield return new EventOccurrence(calendarEvent, calendarEvent.StartDate, calendarEvent.EndDate);
    }

    private static IEnumerable<EventOccurrence> Repeated(
        CalendarEvent calendarEvent, RecurrenceRule rule, DateTime rangeStart, DateTime rangeEnd)
    {
        var duration = calendarEvent.EndDate - calendarEvent.StartDate;
        var index = FirstIndexThatCanReach(calendarEvent.StartDate, duration, rule.Step, rangeStart);

        while (true)
        {
            var startDate = calendarEvent.StartDate + TimeSpan.FromTicks(rule.Step.Ticks * index);
            if (startDate >= rangeEnd || index >= rule.Count)
                yield break;

            var endDate = startDate + duration;
            if (endDate > rangeStart)
                yield return new EventOccurrence(calendarEvent, startDate, endDate);

            index++;
        }
    }

    // Jumps straight to the range instead of walking every occurrence since the series started.
    private static long FirstIndexThatCanReach(DateTime seriesStart, TimeSpan duration, TimeSpan step, DateTime rangeStart)
    {
        var gap = rangeStart - duration - seriesStart;
        return gap <= TimeSpan.Zero ? 0 : gap.Ticks / step.Ticks;
    }
}
