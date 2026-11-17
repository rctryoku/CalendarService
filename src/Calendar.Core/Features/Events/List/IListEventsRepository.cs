namespace Calendar.Core.Features.Events.List;

public interface IListEventsRepository
{
    /// <summary>Events overlapping [rangeStart, rangeEnd), ordered by start date. A null bound is unbounded.</summary>
    Task<IReadOnlyList<CalendarEvent>> ListAsync(
        Guid calendarId, DateTime? rangeStart, DateTime? rangeEnd, CancellationToken cancellationToken);
}
