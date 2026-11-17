namespace Calendar.Core.Features.Events;

public sealed record CalendarEvent(Guid Id, string Title, string? Description, DateTime StartDate, DateTime EndDate)
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
}
