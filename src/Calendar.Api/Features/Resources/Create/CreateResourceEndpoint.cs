using Calendar.Api.Features.Resources.GetById;
using FastEndpoints;

namespace Calendar.Api.Features.Resources.Create;

public sealed class CreateResourceEndpoint : Endpoint<CreateResourceRequest, ResourceResponse>
{
    public override void Configure()
    {
        Post("/");
        Group<ResourcesGroup>();
    }

    public override async Task HandleAsync(CreateResourceRequest req, CancellationToken ct)
    {
        var resource = await req.ToCommand().ExecuteAsync(ct);

        await Send.CreatedAtAsync<GetResourceByIdEndpoint>(
            new { id = resource.Id }, ResourceResponse.From(resource), cancellation: ct);
    }
}
