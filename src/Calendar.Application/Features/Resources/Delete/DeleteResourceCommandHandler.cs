using Calendar.Core.Exceptions;
using Calendar.Core.Features.Resources;
using Calendar.Core.Features.Resources.Delete;
using FastEndpoints;

namespace Calendar.Application.Features.Resources.Delete;

public sealed record DeleteResourceCommand(Guid Id) : ICommand;

public sealed class DeleteResourceCommandHandler(IDeleteResourceRepository repository)
    : ICommandHandler<DeleteResourceCommand>
{
    public async Task ExecuteAsync(DeleteResourceCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var deleted = await repository.DeleteAsync(command.Id, ct);
        if (!deleted)
            throw new NotFoundException(nameof(Resource), command.Id);
    }
}
