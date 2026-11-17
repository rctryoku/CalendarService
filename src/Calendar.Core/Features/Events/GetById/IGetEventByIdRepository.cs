namespace Calendar.Core.Features.Events.GetById;

public interface IGetEventByIdRepository
{
    Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
