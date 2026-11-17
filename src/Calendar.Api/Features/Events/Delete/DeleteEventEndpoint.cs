using FastEndpoints;

namespace Calendar.Api.Features.Events.Delete;

public sealed class DeleteEventEndpoint : Endpoint<DeleteEventRequest>
{
    public override void Configure()
    {
        Delete("/{id:guid}");
        Group<EventsGroup>();
    }

    public override async Task HandleAsync(DeleteEventRequest req, CancellationToken ct)
    {
        await req.ToCommand().ExecuteAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
