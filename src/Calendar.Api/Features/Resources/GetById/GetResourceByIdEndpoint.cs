using FastEndpoints;

namespace Calendar.Api.Features.Resources.GetById;

public sealed class GetResourceByIdEndpoint : Endpoint<GetResourceByIdRequest, ResourceResponse>
{
    public override void Configure()
    {
        Get("/{id:guid}");
        Group<ResourcesGroup>();
    }

    public override async Task HandleAsync(GetResourceByIdRequest req, CancellationToken ct)
    {
        var resource = await req.ToQuery().ExecuteAsync(ct);

        await Send.OkAsync(ResourceResponse.From(resource), ct);
    }
}
