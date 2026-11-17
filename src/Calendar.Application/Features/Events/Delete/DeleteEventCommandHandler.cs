using Calendar.Core.Exceptions;
using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Delete;
using FastEndpoints;

namespace Calendar.Application.Features.Events.Delete;

public sealed record DeleteEventCommand(Guid CalendarId, Guid Id) : ICommand;

public sealed class DeleteEventCommandHandler(IDeleteEventRepository repository)
    : ICommandHandler<DeleteEventCommand>
{
    public async Task ExecuteAsync(DeleteEventCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var deleted = await repository.DeleteAsync(command.CalendarId, command.Id, ct);
        if (!deleted)
            throw new NotFoundException(nameof(CalendarEvent), command.Id);
    }
}
