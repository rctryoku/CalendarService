using Calendar.Application.Features.Events.GetById;
using FastEndpoints;

namespace Calendar.Api.Features.Events.GetById;

public sealed class GetEventByIdEndpoint : Endpoint<GetEventByIdRequest, EventResponse>
{
    public override void Configure()
    {
        Get("/{id:guid}");
        Group<EventsGroup>();
    }

    public override async Task HandleAsync(GetEventByIdRequest req, CancellationToken ct)
    {
        var calendarEvent = await new GetEventByIdQuery(req.CalendarId, req.Id).ExecuteAsync(ct);

        await Send.OkAsync(EventResponse.From(calendarEvent), ct);
    }
}
