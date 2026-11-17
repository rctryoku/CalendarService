namespace Calendar.Core.Features.Calendars;

public sealed record UserCalendar(Guid Id, string Name, string Owner)
{
    public const int NameMaxLength = 100;
    public const int OwnerMaxLength = 100;
}
