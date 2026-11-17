namespace Calendar.Core.Features.Events.Update;

public interface IUpdateEventRepository
{
    Task<bool> UpdateAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken);
}
