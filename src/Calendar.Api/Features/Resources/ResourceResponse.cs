using Calendar.Core.Features.Resources;

namespace Calendar.Api.Features.Resources;

public sealed record ResourceResponse(Guid Id, string Name)
{
    public static ResourceResponse From(Resource resource) => new(resource.Id, resource.Name);
}
