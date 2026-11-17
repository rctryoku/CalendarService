using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Recurrence;

namespace Calendar.UnitTests.Features.Events;

public sealed class EventOccurrencesTests
{
    private static readonly DateTime MondayNine = Utc(2026, 11, 16, 9);

    [Fact]
    public void Between_SingleEventInsideRange_ReturnsTheEvent()
    {
        var calendarEvent = Event(MondayNine, hours: 1);

        var occurrences = EventOccurrences.Between(calendarEvent, Utc(2026, 11, 16), Utc(2026, 11, 17));

        var occurrence = Assert.Single(occurrences);
        Assert.Equal(MondayNine, occurrence.StartDate);
        Assert.Equal(MondayNine.AddHours(1), occurrence.EndDate);
    }

    [Fact]
    public void Between_SingleEventEndingExactlyAtRangeStart_ReturnsNothing()
    {
        var calendarEvent = Event(MondayNine, hours: 1);

        var occurrences = EventOccurrences.Between(calendarEvent, MondayNine.AddHours(1), Utc(2026, 11, 17));

        Assert.Empty(occurrences);
    }

    [Theory]
    [InlineData(RecurrenceFrequency.Daily, 1, 1)]
    [InlineData(RecurrenceFrequency.Daily, 2, 2)]
    [InlineData(RecurrenceFrequency.Weekly, 1, 7)]
    [InlineData(RecurrenceFrequency.Weekly, 2, 14)]
    public void Between_Recurring_RepeatsEveryIntervalDaysOrWeeks(
        RecurrenceFrequency frequency, int interval, int daysBetween)
    {
        var calendarEvent = Event(MondayNine, hours: 1, Rule(frequency, interval));

        var occurrences = EventOccurrences.Between(calendarEvent, Utc(2026, 11, 16), Utc(2026, 11, 16).AddDays(daysBetween * 3));

        Assert.Equal(
            [MondayNine, MondayNine.AddDays(daysBetween), MondayNine.AddDays(daysBetween * 2)],
            StartDates(occurrences));
    }

    [Fact]
    public void Between_CountAndRangeStartingMidSeries_CountsFromFirstOccurrence()
    {
        var calendarEvent = Event(MondayNine, hours: 1, Rule(RecurrenceFrequency.Daily, count: 5));

        var occurrences = EventOccurrences.Between(calendarEvent, Utc(2026, 11, 19), Utc(2026, 12, 31));

        Assert.Equal([MondayNine.AddDays(3), MondayNine.AddDays(4)], StartDates(occurrences));
    }

    [Fact]
    public void Between_RangeBeforeSeriesStarts_ReturnsNothing()
    {
        var calendarEvent = Event(MondayNine, hours: 1, Rule(RecurrenceFrequency.Daily));

        var occurrences = EventOccurrences.Between(calendarEvent, Utc(2026, 11, 1), Utc(2026, 11, 16));

        Assert.Empty(occurrences);
    }

    [Fact]
    public void Between_OccurrenceSpanningRangeStart_IsIncluded()
    {
        var lateEvening = Utc(2026, 11, 16, 23);
        var calendarEvent = Event(lateEvening, hours: 2, Rule(RecurrenceFrequency.Daily));

        var occurrences = EventOccurrences.Between(calendarEvent, Utc(2026, 11, 17), Utc(2026, 11, 18));

        Assert.Equal([lateEvening, lateEvening.AddDays(1)], StartDates(occurrences));
    }

    [Fact]
    public void Between_RangeYearsAfterSeriesStart_JumpsToTheRightOccurrences()
    {
        var calendarEvent = Event(MondayNine, hours: 1, Rule(RecurrenceFrequency.Weekly));

        var occurrences = EventOccurrences.Between(calendarEvent, Utc(2036, 1, 1), Utc(2036, 1, 15));

        Assert.Equal([Utc(2036, 1, 7, 9), Utc(2036, 1, 14, 9)], StartDates(occurrences));
    }

    private static CalendarEvent Event(DateTime startDate, int hours, RecurrenceRule? recurrence = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Standup", null, startDate, startDate.AddHours(hours), recurrence, []);

    private static RecurrenceRule Rule(RecurrenceFrequency frequency, int interval = 1, int? count = null) =>
        new(frequency, interval, count);

    private static DateTime Utc(int year, int month, int day, int hour = 0) =>
        new(year, month, day, hour, 0, 0, DateTimeKind.Utc);

    private static IEnumerable<DateTime> StartDates(IEnumerable<EventOccurrence> occurrences) =>
        occurrences.Select(occurrence => occurrence.StartDate);
}
