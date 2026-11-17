namespace Calendar.Api.Features.Events.GetById;

public sealed record GetEventByIdRequest
{
    public Guid Id { get; init; }
}
