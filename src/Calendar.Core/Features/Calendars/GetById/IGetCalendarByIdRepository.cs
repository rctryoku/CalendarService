namespace Calendar.Core.Features.Calendars.GetById;

public interface IGetCalendarByIdRepository
{
    Task<UserCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
