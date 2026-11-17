namespace Calendar.Core.Features.Events.Recurrence;

/// <param name="Frequency">Repeat every day or every week.</param>
/// <param name="Interval">Repeat every Interval days or weeks (1 = every, 2 = every other).</param>
/// <param name="Count">The total number of occurrences, counted from the first one. Null repeats forever.</param>
public sealed record RecurrenceRule(RecurrenceFrequency Frequency, int Interval, int? Count)
{
    public const int MaxInterval = 99;
    public const int MaxCount = 1000;

    public TimeSpan Step => Frequency == RecurrenceFrequency.Weekly
        ? TimeSpan.FromDays(7 * Interval)
        : TimeSpan.FromDays(Interval);
}
