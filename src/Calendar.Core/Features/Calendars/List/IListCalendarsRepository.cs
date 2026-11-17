namespace Calendar.Core.Features.Calendars.List;

public interface IListCalendarsRepository
{
    Task<IReadOnlyList<UserCalendar>> ListAsync(string? owner, CancellationToken cancellationToken);
}
