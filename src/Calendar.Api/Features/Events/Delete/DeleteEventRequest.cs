namespace Calendar.Api.Features.Events.Delete;

public sealed record DeleteEventRequest
{
    public Guid Id { get; init; }
}
