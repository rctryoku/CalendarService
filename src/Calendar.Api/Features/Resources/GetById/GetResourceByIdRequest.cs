using Calendar.Application.Features.Resources.GetById;

namespace Calendar.Api.Features.Resources.GetById;

public sealed record GetResourceByIdRequest
{
    public Guid Id { get; init; }

    public GetResourceByIdQuery ToQuery() => new(Id);
}
