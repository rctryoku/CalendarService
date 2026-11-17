namespace Calendar.Core.Features.Calendars.Create;

public interface ICreateCalendarRepository
{
    Task CreateAsync(UserCalendar calendar, CancellationToken cancellationToken);
}
