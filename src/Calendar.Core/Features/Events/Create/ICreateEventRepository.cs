namespace Calendar.Core.Features.Events.Create;

public interface ICreateEventRepository
{
    Task CreateAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken);
}
