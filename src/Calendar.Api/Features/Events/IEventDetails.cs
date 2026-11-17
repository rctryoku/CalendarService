namespace Calendar.Api.Features.Events;

public interface IEventDetails
{
    string Title { get; }

    string? Description { get; }

    DateTimeOffset StartDate { get; }

    DateTimeOffset EndDate { get; }

    RecurrenceRequest? Recurrence { get; }
}
