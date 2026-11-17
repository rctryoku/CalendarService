using Calendar.Core.Features.Events.Recurrence;

namespace Calendar.Api.Features.Events;

public sealed record RecurrenceRequest
{
    public RecurrenceFrequency Frequency { get; init; }

    public int Interval { get; init; } = 1;

    public int? Count { get; init; }

    public RecurrenceRule ToRule() => new(Frequency, Interval, Count);
}
