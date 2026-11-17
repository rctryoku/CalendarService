namespace Calendar.Core.Features.Resources.GetById;

public interface IGetResourceByIdRepository
{
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
