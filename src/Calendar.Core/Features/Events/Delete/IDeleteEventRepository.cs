namespace Calendar.Core.Features.Events.Delete;

public interface IDeleteEventRepository
{
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
