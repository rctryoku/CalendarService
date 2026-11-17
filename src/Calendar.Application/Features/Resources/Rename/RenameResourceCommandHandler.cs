using Calendar.Core.Exceptions;
using Calendar.Core.Features.Resources;
using Calendar.Core.Features.Resources.Rename;
using FastEndpoints;

namespace Calendar.Application.Features.Resources.Rename;

public sealed record RenameResourceCommand(Guid Id, string Name) : ICommand<Resource>;

public sealed class RenameResourceCommandHandler(IRenameResourceRepository repository)
    : ICommandHandler<RenameResourceCommand, Resource>
{
    public async Task<Resource> ExecuteAsync(RenameResourceCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        return await repository.RenameAsync(command.Id, command.Name, ct)
            ?? throw new NotFoundException(nameof(Resource), command.Id);
    }
}
