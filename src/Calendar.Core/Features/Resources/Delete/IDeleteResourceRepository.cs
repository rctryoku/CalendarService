namespace Calendar.Core.Features.Resources.Delete;

public interface IDeleteResourceRepository
{
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
