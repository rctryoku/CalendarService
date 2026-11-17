using Calendar.Core.Features.Calendars;

namespace Calendar.Api.Features.Calendars;

public sealed record CalendarResponse(Guid Id, string Name, string Owner)
{
    public static CalendarResponse From(UserCalendar calendar) => new(calendar.Id, calendar.Name, calendar.Owner);
}
