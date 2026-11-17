using Calendar.Core.Features.Events.Recurrence;

namespace Calendar.Api.Features.Events;

public sealed record RecurrenceResponse(RecurrenceFrequency Frequency, int Interval, int? Count)
{
    public static RecurrenceResponse? From(RecurrenceRule? rule) =>
        rule is null ? null : new(rule.Frequency, rule.Interval, rule.Count);
}
