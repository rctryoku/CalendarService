using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Bookings;
using Calendar.Core.Features.Events.Recurrence;

namespace Calendar.UnitTests.Features.Events;

public sealed class BookingConflictsTests
{
    private static readonly Guid MeetingRoom = Guid.NewGuid();
    private static readonly Guid Projector = Guid.NewGuid();
    private static readonly DateTime MondayNine = new(2026, 11, 16, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void FindFirst_OverlappingEventWithSameResource_ReturnsConflict()
    {
        var existing = Event("Planning", MondayNine, [MeetingRoom]);
        var candidate = Event("Standup", MondayNine.AddMinutes(30), [MeetingRoom]);

        var conflict = BookingConflicts.FindFirst(candidate, [existing]);

        Assert.NotNull(conflict);
        Assert.Equal(existing, conflict.ConflictingEvent);
        Assert.Equal(MeetingRoom, conflict.ResourceId);
        Assert.Equal(MondayNine, conflict.StartDate);
    }

    [Fact]
    public void FindFirst_OverlappingEventWithOtherResource_ReturnsNull()
    {
        var existing = Event("Planning", MondayNine, [Projector]);
        var candidate = Event("Standup", MondayNine, [MeetingRoom]);

        var conflict = BookingConflicts.FindFirst(candidate, [existing]);

        Assert.Null(conflict);
    }

    [Fact]
    public void FindFirst_BackToBackEvents_ReturnsNull()
    {
        var existing = Event("Planning", MondayNine, [MeetingRoom]);
        var candidate = Event("Standup", MondayNine.AddHours(1), [MeetingRoom]);

        var conflict = BookingConflicts.FindFirst(candidate, [existing]);

        Assert.Null(conflict);
    }

    [Fact]
    public void FindFirst_EventItself_IsNotAConflict()
    {
        var existing = Event("Planning", MondayNine, [MeetingRoom]);
        var moved = existing with { StartDate = MondayNine.AddMinutes(30), EndDate = MondayNine.AddMinutes(90) };

        var conflict = BookingConflicts.FindFirst(moved, [existing]);

        Assert.Null(conflict);
    }

    [Fact]
    public void FindFirst_WeeklySeriesHittingLaterSingleEvent_ReturnsThatOccurrence()
    {
        var thirdMonday = MondayNine.AddDays(14);
        var existing = Event("Workshop", thirdMonday.AddMinutes(30), [MeetingRoom]);
        var candidate = Event("Team meeting", MondayNine, [MeetingRoom], Weekly());

        var conflict = BookingConflicts.FindFirst(candidate, [existing]);

        Assert.NotNull(conflict);
        Assert.Equal(thirdMonday.AddMinutes(30), conflict.StartDate);
    }

    [Fact]
    public void FindFirst_SingleEventHittingExistingSeries_ReturnsConflict()
    {
        var existing = Event("Team meeting", MondayNine, [MeetingRoom], Weekly());
        var candidate = Event("Workshop", MondayNine.AddDays(70), [MeetingRoom]);

        var conflict = BookingConflicts.FindFirst(candidate, [existing]);

        Assert.NotNull(conflict);
        Assert.Equal(MondayNine.AddDays(70), conflict.StartDate);
    }

    [Fact]
    public void FindFirst_SeriesEndedByCountBeforeOtherEvent_ReturnsNull()
    {
        var existing = Event("Workshop", MondayNine.AddDays(21), [MeetingRoom]);
        var candidate = Event("Team meeting", MondayNine, [MeetingRoom], Weekly(count: 3));

        var conflict = BookingConflicts.FindFirst(candidate, [existing]);

        Assert.Null(conflict);
    }

    [Fact]
    public void FindFirst_TwoSeriesOnDifferentDays_ReturnsNull()
    {
        var existing = Event("Monday meeting", MondayNine, [MeetingRoom], Weekly());
        var candidate = Event("Tuesday meeting", MondayNine.AddDays(1), [MeetingRoom], Weekly());

        var conflict = BookingConflicts.FindFirst(candidate, [existing]);

        Assert.Null(conflict);
    }

    [Fact]
    public void FindFirst_OpenEndedSeries_IsCheckedOnlyWithinHorizon()
    {
        var insideHorizon = Event("Inside", MondayNine.AddDays(7 * 50), [MeetingRoom]);
        var outsideHorizon = Event("Outside", MondayNine.AddDays(7 * 60), [MeetingRoom]);
        var candidate = Event("Team meeting", MondayNine, [MeetingRoom], Weekly());

        Assert.NotNull(BookingConflicts.FindFirst(candidate, [insideHorizon]));
        Assert.Null(BookingConflicts.FindFirst(candidate, [outsideHorizon]));
    }

    [Fact]
    public void CheckWindow_SeriesWithCount_EndsWhenLastOccurrenceEnds()
    {
        var calendarEvent = Event("Team meeting", MondayNine, [MeetingRoom], Weekly(count: 3));

        var (start, end) = BookingConflicts.CheckWindow(calendarEvent);

        Assert.Equal(MondayNine, start);
        Assert.Equal(MondayNine.AddDays(14).AddHours(1), end);
    }

    private static CalendarEvent Event(
        string title, DateTime startDate, IReadOnlyList<Guid> resourceIds, RecurrenceRule? recurrence = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), title, null, startDate, startDate.AddHours(1), recurrence, resourceIds);

    private static RecurrenceRule Weekly(int? count = null) => new(RecurrenceFrequency.Weekly, 1, count);
}
