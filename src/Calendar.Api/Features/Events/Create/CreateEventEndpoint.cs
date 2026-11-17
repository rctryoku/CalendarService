using Calendar.Api.Features.Events.GetById;
using FastEndpoints;

namespace Calendar.Api.Features.Events.Create;

public sealed class CreateEventEndpoint : Endpoint<CreateEventRequest, EventResponse>
{
    public override void Configure()
    {
        Post("/api/events");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateEventRequest req, CancellationToken ct)
    {
        var calendarEvent = await req.ToCommand().ExecuteAsync(ct);

        await Send.CreatedAtAsync<GetEventByIdEndpoint>(
            new { id = calendarEvent.Id }, EventResponse.From(calendarEvent), cancellation: ct);
    }
}
