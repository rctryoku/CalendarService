using Calendar.Core.Features.Resources;
using Calendar.Core.Features.Resources.Create;
using FastEndpoints;

namespace Calendar.Application.Features.Resources.Create;

public sealed record CreateResourceCommand(string Name) : ICommand<Resource>;

public sealed class CreateResourceCommandHandler(ICreateResourceRepository repository)
    : ICommandHandler<CreateResourceCommand, Resource>
{
    public async Task<Resource> ExecuteAsync(CreateResourceCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var resource = new Resource(Guid.CreateVersion7(), command.Name);
        await repository.CreateAsync(resource, ct);

        return resource;
    }
}
