using Calendar.Core.Exceptions;
using Calendar.Core.Features.Calendars;
using Calendar.Core.Features.Calendars.GetById;

namespace Calendar.Application.Features.Calendars;

internal static class CalendarRepositoryExtensions
{
    public static async Task EnsureExistsAsync(
        this IGetCalendarByIdRepository calendars, Guid calendarId, CancellationToken cancellationToken)
    {
        var calendar = await calendars.GetByIdAsync(calendarId, cancellationToken);
        if (calendar is null)
            throw new NotFoundException(nameof(UserCalendar), calendarId);
    }
}
