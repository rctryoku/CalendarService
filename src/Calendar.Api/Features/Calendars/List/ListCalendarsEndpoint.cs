using FastEndpoints;

namespace Calendar.Api.Features.Calendars.List;

public sealed class ListCalendarsEndpoint : Endpoint<ListCalendarsRequest, IReadOnlyList<CalendarResponse>>
{
    public override void Configure()
    {
        Get("/api/calendars");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListCalendarsRequest req, CancellationToken ct)
    {
        var calendars = await req.ToQuery().ExecuteAsync(ct);

        await Send.OkAsync([.. calendars.Select(CalendarResponse.From)], ct);
    }
}
