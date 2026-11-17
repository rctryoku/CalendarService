using FastEndpoints;

namespace Calendar.Api.Features.Resources.Rename;

public sealed class RenameResourceEndpoint : Endpoint<RenameResourceRequest, ResourceResponse>
{
    public override void Configure()
    {
        Put("/{id:guid}");
        Group<ResourcesGroup>();
    }

    public override async Task HandleAsync(RenameResourceRequest req, CancellationToken ct)
    {
        var resource = await req.ToCommand().ExecuteAsync(ct);

        await Send.OkAsync(ResourceResponse.From(resource), ct);
    }
}
