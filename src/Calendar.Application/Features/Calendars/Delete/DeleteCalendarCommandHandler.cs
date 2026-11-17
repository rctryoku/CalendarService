using Calendar.Core.Exceptions;
using Calendar.Core.Features.Calendars;
using Calendar.Core.Features.Calendars.Delete;
using FastEndpoints;

namespace Calendar.Application.Features.Calendars.Delete;

public sealed record DeleteCalendarCommand(Guid Id) : ICommand;

public sealed class DeleteCalendarCommandHandler(IDeleteCalendarRepository repository)
    : ICommandHandler<DeleteCalendarCommand>
{
    public async Task ExecuteAsync(DeleteCalendarCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var deleted = await repository.DeleteAsync(command.Id, ct);
        if (!deleted)
            throw new NotFoundException(nameof(UserCalendar), command.Id);
    }
}
