using Calendar.Application.Features.Resources.Delete;
using FastEndpoints;

namespace Calendar.Api.Features.Resources.Delete;

public sealed class DeleteResourceEndpoint : Endpoint<DeleteResourceRequest>
{
    public override void Configure()
    {
        Delete("/{id:guid}");
        Group<ResourcesGroup>();
    }

    public override async Task HandleAsync(DeleteResourceRequest req, CancellationToken ct)
    {
        await new DeleteResourceCommand(req.Id).ExecuteAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
