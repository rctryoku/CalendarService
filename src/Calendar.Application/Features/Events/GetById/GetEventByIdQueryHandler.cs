using Calendar.Core.Exceptions;
using Calendar.Core.Features.Events;
using Calendar.Core.Features.Events.GetById;
using FastEndpoints;

namespace Calendar.Application.Features.Events.GetById;

public sealed record GetEventByIdQuery(Guid CalendarId, Guid Id) : ICommand<CalendarEvent>;

public sealed class GetEventByIdQueryHandler(IGetEventByIdRepository repository)
    : ICommandHandler<GetEventByIdQuery, CalendarEvent>
{
    public async Task<CalendarEvent> ExecuteAsync(GetEventByIdQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await repository.GetByIdAsync(query.CalendarId, query.Id, ct)
            ?? throw new NotFoundException(nameof(CalendarEvent), query.Id);
    }
}
