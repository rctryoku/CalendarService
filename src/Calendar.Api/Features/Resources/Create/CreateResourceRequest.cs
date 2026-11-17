using Calendar.Application.Features.Resources.Create;

namespace Calendar.Api.Features.Resources.Create;

public sealed record CreateResourceRequest
{
    public string Name { get; init; } = "";

    public CreateResourceCommand ToCommand() => new(Name);
}
