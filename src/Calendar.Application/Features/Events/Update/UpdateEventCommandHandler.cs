using Calendar.Core.Exceptions;
using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Recurrence;
using Calendar.Core.Features.Events.Update;
using FastEndpoints;

namespace Calendar.Application.Features.Events.Update;

public sealed record UpdateEventCommand(
    Guid CalendarId,
    Guid Id,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    RecurrenceRule? Recurrence) : ICommand<CalendarEvent>;

public sealed class UpdateEventCommandHandler(IUpdateEventRepository repository)
    : ICommandHandler<UpdateEventCommand, CalendarEvent>
{
    public async Task<CalendarEvent> ExecuteAsync(UpdateEventCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var calendarEvent = new CalendarEvent(
            command.Id,
            command.CalendarId,
            command.Title,
            command.Description,
            command.StartDate,
            command.EndDate,
            command.Recurrence);

        var updated = await repository.UpdateAsync(calendarEvent, ct);
        if (!updated)
            throw new NotFoundException(nameof(CalendarEvent), command.Id);

        return calendarEvent;
    }
}
