using Calendar.Core.Exceptions;
using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Update;
using FastEndpoints;

namespace Calendar.Application.Features.Events.Update;

public sealed record UpdateEventCommand(Guid Id, string Title, string? Description, DateTime StartDate, DateTime EndDate)
    : ICommand<CalendarEvent>;

public sealed class UpdateEventCommandHandler(IUpdateEventRepository repository)
    : ICommandHandler<UpdateEventCommand, CalendarEvent>
{
    public async Task<CalendarEvent> ExecuteAsync(UpdateEventCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var calendarEvent = new CalendarEvent(
            command.Id, command.Title, command.Description, command.StartDate, command.EndDate);

        var updated = await repository.UpdateAsync(calendarEvent, ct);
        if (!updated)
            throw new NotFoundException(nameof(CalendarEvent), command.Id);

        return calendarEvent;
    }
}
