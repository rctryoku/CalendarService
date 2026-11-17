namespace Calendar.Core.Features.Resources.Create;

public interface ICreateResourceRepository
{
    Task CreateAsync(Resource resource, CancellationToken cancellationToken);
}
