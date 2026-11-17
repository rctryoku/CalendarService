using FastEndpoints;

namespace Calendar.Api.Features.Events.List;

public sealed class ListEventsEndpoint : Endpoint<ListEventsRequest, IReadOnlyList<EventOccurrenceResponse>>
{
    public override void Configure()
    {
        Get("/");
        Group<EventsGroup>();
    }

    public override async Task HandleAsync(ListEventsRequest req, CancellationToken ct)
    {
        var occurrences = await req.ToQuery().ExecuteAsync(ct);

        await Send.OkAsync([.. occurrences.Select(EventOccurrenceResponse.From)], ct);
    }
}
