namespace Calendar.Core.Features.Resources.Rename;

public interface IRenameResourceRepository
{
    Task<Resource?> RenameAsync(Guid id, string name, CancellationToken cancellationToken);
}
