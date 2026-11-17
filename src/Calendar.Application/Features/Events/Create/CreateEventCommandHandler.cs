using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Create;
using FastEndpoints;

namespace Calendar.Application.Features.Events.Create;

public sealed record CreateEventCommand(string Title, string? Description, DateTime StartDate, DateTime EndDate)
    : ICommand<CalendarEvent>;

public sealed class CreateEventCommandHandler(ICreateEventRepository repository)
    : ICommandHandler<CreateEventCommand, CalendarEvent>
{
    public async Task<CalendarEvent> ExecuteAsync(CreateEventCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var calendarEvent = new CalendarEvent(
            Guid.CreateVersion7(), command.Title, command.Description, command.StartDate, command.EndDate);
        await repository.CreateAsync(calendarEvent, ct);

        return calendarEvent;
    }
}
