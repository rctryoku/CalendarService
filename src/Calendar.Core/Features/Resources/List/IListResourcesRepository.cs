namespace Calendar.Core.Features.Resources.List;

public interface IListResourcesRepository
{
    Task<IReadOnlyList<Resource>> ListAsync(CancellationToken cancellationToken);
}
