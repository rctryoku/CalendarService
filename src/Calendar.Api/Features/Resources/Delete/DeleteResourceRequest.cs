namespace Calendar.Api.Features.Resources.Delete;

public sealed record DeleteResourceRequest
{
    public Guid Id { get; init; }
}
