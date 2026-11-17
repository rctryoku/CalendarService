namespace Calendar.Api.Features.Resources.GetById;

public sealed record GetResourceByIdRequest
{
    public Guid Id { get; init; }
}
