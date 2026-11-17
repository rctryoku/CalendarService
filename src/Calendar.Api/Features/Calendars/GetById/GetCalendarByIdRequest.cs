namespace Calendar.Api.Features.Calendars.GetById;

public sealed record GetCalendarByIdRequest
{
    public Guid Id { get; init; }
}
