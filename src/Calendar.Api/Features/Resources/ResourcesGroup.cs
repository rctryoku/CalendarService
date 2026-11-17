using FastEndpoints;

namespace Calendar.Api.Features.Resources;

public sealed class ResourcesGroup : Group
{
    public ResourcesGroup() =>
        Configure("api/resources", endpoint =>
        {
            endpoint.AllowAnonymous();
            endpoint.Description(builder => builder.WithTags("Resources"));
        });
}
