using Calendar.Core.Exceptions;
using Calendar.Core.Features.Calendars;
using Calendar.Core.Features.Calendars.Rename;
using FastEndpoints;

namespace Calendar.Application.Features.Calendars.Rename;

public sealed record RenameCalendarCommand(Guid Id, string Name) : ICommand<UserCalendar>;

public sealed class RenameCalendarCommandHandler(IRenameCalendarRepository repository)
    : ICommandHandler<RenameCalendarCommand, UserCalendar>
{
    public async Task<UserCalendar> ExecuteAsync(RenameCalendarCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        return await repository.RenameAsync(command.Id, command.Name, ct)
            ?? throw new NotFoundException(nameof(UserCalendar), command.Id);
    }
}
