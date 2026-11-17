using Calendar.Application.Features.Events.Bookings;
using Calendar.Core.Exceptions;
using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Bookings;
using Calendar.Core.Features.Events.GetById;
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
    RecurrenceRule? Recurrence,
    IReadOnlyList<Guid> ResourceIds) : ICommand<CalendarEvent>;

public sealed class UpdateEventCommandHandler(
    IUpdateEventRepository repository,
    IGetEventByIdRepository events,
    IResourceBookingRepository bookings)
    : ICommandHandler<UpdateEventCommand, CalendarEvent>
{
    public async Task<CalendarEvent> ExecuteAsync(UpdateEventCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existing = await events.GetByIdAsync(command.CalendarId, command.Id, ct);
        if (existing is null)
            throw new NotFoundException(nameof(CalendarEvent), command.Id);

        var calendarEvent = existing with
        {
            Title = command.Title,
            Description = command.Description,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Recurrence = command.Recurrence,
            ResourceIds = command.ResourceIds,
        };
        await bookings.EnsureResourcesAreFreeAsync(calendarEvent, ct);

        var updated = await repository.UpdateAsync(calendarEvent, ct);
        if (!updated)
            throw new NotFoundException(nameof(CalendarEvent), command.Id);

        return calendarEvent;
    }
}
