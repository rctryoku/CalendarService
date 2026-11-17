using Calendar.Application.Features.Calendars;
using Calendar.Application.Features.Events.Bookings;
using Calendar.Core.Features.Calendars.GetById;
using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.Bookings;
using Calendar.Core.Features.Events.Create;
using Calendar.Core.Features.Events.Recurrence;
using FastEndpoints;

namespace Calendar.Application.Features.Events.Create;

public sealed record CreateEventCommand(
    Guid CalendarId,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    RecurrenceRule? Recurrence,
    IReadOnlyList<Guid> ResourceIds) : ICommand<CalendarEvent>;

public sealed class CreateEventCommandHandler(
    ICreateEventRepository repository,
    IGetCalendarByIdRepository calendars,
    IResourceBookingRepository bookings)
    : ICommandHandler<CreateEventCommand, CalendarEvent>
{
    public async Task<CalendarEvent> ExecuteAsync(CreateEventCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        await calendars.EnsureExistsAsync(command.CalendarId, ct);

        var calendarEvent = new CalendarEvent(
            Guid.CreateVersion7(),
            command.CalendarId,
            command.Title,
            command.Description,
            command.StartDate,
            command.EndDate,
            command.Recurrence,
            command.ResourceIds);
        await bookings.EnsureResourcesAreFreeAsync(calendarEvent, ct);
        await repository.CreateAsync(calendarEvent, ct);

        return calendarEvent;
    }
}
