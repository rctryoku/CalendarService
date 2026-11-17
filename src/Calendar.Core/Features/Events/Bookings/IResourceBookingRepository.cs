using Calendar.Core.Features.Resources;

namespace Calendar.Core.Features.Events.Bookings;

public interface IResourceBookingRepository
{
    Task<IReadOnlyList<Resource>> GetResourcesAsync(
        IReadOnlyCollection<Guid> resourceIds, CancellationToken cancellationToken);

    /// <summary>
    /// Events in any calendar, except <paramref name="excludeEventId"/>, that book one of the resources and can
    /// have an occurrence in [rangeStart, rangeEnd).
    /// </summary>
    Task<IReadOnlyList<CalendarEvent>> ListBookingsAsync(
        IReadOnlyCollection<Guid> resourceIds,
        Guid excludeEventId,
        DateTime rangeStart,
        DateTime rangeEnd,
        CancellationToken cancellationToken);
}
