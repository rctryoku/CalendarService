using Calendar.Core.Features.Resources;
using Calendar.Core.Features.Resources.List;
using FastEndpoints;

namespace Calendar.Application.Features.Resources.List;

public sealed record ListResourcesQuery : ICommand<IReadOnlyList<Resource>>;

public sealed class ListResourcesQueryHandler(IListResourcesRepository repository)
    : ICommandHandler<ListResourcesQuery, IReadOnlyList<Resource>>
{
    public Task<IReadOnlyList<Resource>> ExecuteAsync(ListResourcesQuery query, CancellationToken ct) =>
        repository.ListAsync(ct);
}
