using Calendar.Application.Features.Resources.List;
using FastEndpoints;

namespace Calendar.Api.Features.Resources.List;

public sealed class ListResourcesEndpoint : EndpointWithoutRequest<IReadOnlyList<ResourceResponse>>
{
    public override void Configure()
    {
        Get("/");
        Group<ResourcesGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var resources = await new ListResourcesQuery().ExecuteAsync(ct);

        await Send.OkAsync([.. resources.Select(ResourceResponse.From)], ct);
    }
}
