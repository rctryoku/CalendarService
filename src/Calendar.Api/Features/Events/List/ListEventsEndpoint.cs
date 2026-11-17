using FastEndpoints;

namespace Calendar.Api.Features.Events.List;

public sealed class ListEventsEndpoint : Endpoint<ListEventsRequest, IReadOnlyList<EventResponse>>
{
    public override void Configure()
    {
        Get("/api/events");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListEventsRequest req, CancellationToken ct)
    {
        var events = await req.ToQuery().ExecuteAsync(ct);

        await Send.OkAsync([.. events.Select(EventResponse.From)], ct);
    }
}
