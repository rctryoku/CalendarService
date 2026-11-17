using Calendar.Application.Features.Events.Delete;
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
        await new DeleteEventCommand(req.CalendarId, req.Id).ExecuteAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
