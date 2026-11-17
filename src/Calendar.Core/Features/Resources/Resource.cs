namespace Calendar.Core.Features.Resources;

public sealed record Resource(Guid Id, string Name)
{
    public const int NameMaxLength = 100;
}
