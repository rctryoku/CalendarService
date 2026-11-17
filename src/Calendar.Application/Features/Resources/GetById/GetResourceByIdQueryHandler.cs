using Calendar.Core.Exceptions;
using Calendar.Core.Features.Resources;
using Calendar.Core.Features.Resources.GetById;
using FastEndpoints;

namespace Calendar.Application.Features.Resources.GetById;

public sealed record GetResourceByIdQuery(Guid Id) : ICommand<Resource>;

public sealed class GetResourceByIdQueryHandler(IGetResourceByIdRepository repository)
    : ICommandHandler<GetResourceByIdQuery, Resource>
{
    public async Task<Resource> ExecuteAsync(GetResourceByIdQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await repository.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException(nameof(Resource), query.Id);
    }
}
