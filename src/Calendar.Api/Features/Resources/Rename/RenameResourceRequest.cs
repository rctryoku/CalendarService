using Calendar.Application.Features.Resources.Rename;

namespace Calendar.Api.Features.Resources.Rename;

public sealed record RenameResourceRequest
{
    public Guid Id { get; init; }

    public string Name { get; init; } = "";

    public RenameResourceCommand ToCommand() => new(Id, Name);
}
