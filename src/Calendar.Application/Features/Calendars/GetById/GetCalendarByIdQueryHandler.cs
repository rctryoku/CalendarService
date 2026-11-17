using Calendar.Core.Exceptions;
using Calendar.Core.Features.Calendars;
using Calendar.Core.Features.Calendars.GetById;
using FastEndpoints;

namespace Calendar.Application.Features.Calendars.GetById;

public sealed record GetCalendarByIdQuery(Guid Id) : ICommand<UserCalendar>;

public sealed class GetCalendarByIdQueryHandler(IGetCalendarByIdRepository repository)
    : ICommandHandler<GetCalendarByIdQuery, UserCalendar>
{
    public async Task<UserCalendar> ExecuteAsync(GetCalendarByIdQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await repository.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException(nameof(UserCalendar), query.Id);
    }
}
