using Calendar.Core.Features.Calendars;
using Calendar.Core.Features.Calendars.Create;
using FastEndpoints;

namespace Calendar.Application.Features.Calendars.Create;

public sealed record CreateCalendarCommand(string Name, string Owner) : ICommand<UserCalendar>;

public sealed class CreateCalendarCommandHandler(ICreateCalendarRepository repository)
    : ICommandHandler<CreateCalendarCommand, UserCalendar>
{
    public async Task<UserCalendar> ExecuteAsync(CreateCalendarCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var calendar = new UserCalendar(Guid.CreateVersion7(), command.Name, command.Owner);
        await repository.CreateAsync(calendar, ct);

        return calendar;
    }
}
