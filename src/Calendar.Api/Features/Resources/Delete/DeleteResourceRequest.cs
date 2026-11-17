using Calendar.Application.Features.Resources.Delete;

namespace Calendar.Api.Features.Resources.Delete;

public sealed record DeleteResourceRequest
{
    public Guid Id { get; init; }

    public DeleteResourceCommand ToCommand() => new(Id);
}
