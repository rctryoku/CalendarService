namespace Calendar.Core.Features.Calendars.Rename;

public interface IRenameCalendarRepository
{
    Task<UserCalendar?> RenameAsync(Guid id, string name, CancellationToken cancellationToken);
}
