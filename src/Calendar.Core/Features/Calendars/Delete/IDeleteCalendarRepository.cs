namespace Calendar.Core.Features.Calendars.Delete;

public interface IDeleteCalendarRepository
{
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
