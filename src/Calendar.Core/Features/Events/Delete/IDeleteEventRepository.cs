namespace Calendar.Core.Features.Events.Delete;

public interface IDeleteEventRepository
{
    Task<bool> DeleteAsync(Guid calendarId, Guid id, CancellationToken cancellationToken);
}
