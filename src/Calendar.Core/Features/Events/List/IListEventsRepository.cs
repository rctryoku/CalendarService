namespace Calendar.Core.Features.Events.List;

public interface IListEventsRepository
{
    /// <summary>
    /// Events that can have an occurrence in [rangeStart, rangeEnd): single events that overlap it, and
    /// recurring events that start before it ends. Expand with <c>EventOccurrences.Between</c>.
    /// </summary>
    Task<IReadOnlyList<CalendarEvent>> ListAsync(
        Guid calendarId, DateTime rangeStart, DateTime rangeEnd, CancellationToken cancellationToken);
}
