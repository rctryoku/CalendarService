using Calendar.Application.Features.Calendars.GetById;

namespace Calendar.Api.Features.Calendars.GetById;

public sealed record GetCalendarByIdRequest
{
    public Guid Id { get; init; }

    public GetCalendarByIdQuery ToQuery() => new(Id);
}
