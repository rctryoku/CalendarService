using FastEndpoints;

namespace Calendar.Api.Features.Events.Update;

public sealed class UpdateEventEndpoint : Endpoint<UpdateEventRequest, EventResponse>
{
    public override void Configure()
    {
        Put("/api/calendars/{calendarId:guid}/events/{id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateEventRequest req, CancellationToken ct)
    {
        var calendarEvent = await req.ToCommand().ExecuteAsync(ct);

        await Send.OkAsync(EventResponse.From(calendarEvent), ct);
    }
}
