using System.Globalization;
using Calendar.Core.Exceptions;
using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Bookings;
using Calendar.Core.Features.Resources;

namespace Calendar.Application.Features.Events.Bookings;

internal static class ResourceBookingRepositoryExtensions
{
    public static async Task EnsureResourcesAreFreeAsync(
        this IResourceBookingRepository bookings, CalendarEvent calendarEvent, CancellationToken cancellationToken)
    {
        if (calendarEvent.ResourceIds.Count == 0)
            return;

        var resources = await bookings.GetResourcesAsync(calendarEvent.ResourceIds, cancellationToken);
        var missingId = calendarEvent.ResourceIds.Except(resources.Select(resource => resource.Id)).FirstOrDefault();
        if (missingId != Guid.Empty)
            throw new NotFoundException(nameof(Resource), missingId);

        var (start, end) = BookingConflicts.CheckWindow(calendarEvent);
        var otherBookings = await bookings.ListBookingsAsync(
            calendarEvent.ResourceIds, calendarEvent.Id, start, end, cancellationToken);
        if (BookingConflicts.FindFirst(calendarEvent, otherBookings) is not { } conflict)
            return;

        var resourceName = resources.First(resource => resource.Id == conflict.ResourceId).Name;
        throw new ConflictException(string.Create(
            CultureInfo.InvariantCulture,
            $"'{resourceName}' is already booked by '{conflict.ConflictingEvent.Title}' at {conflict.StartDate:u}."));
    }
}
