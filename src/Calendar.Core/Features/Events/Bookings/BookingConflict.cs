namespace Calendar.Core.Features.Events.Bookings;

public sealed record BookingConflict(CalendarEvent ConflictingEvent, Guid ResourceId, DateTime StartDate);
