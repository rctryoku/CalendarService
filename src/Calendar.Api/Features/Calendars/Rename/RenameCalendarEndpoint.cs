using FastEndpoints;

namespace Calendar.Api.Features.Calendars.Rename;

public sealed class RenameCalendarEndpoint : Endpoint<RenameCalendarRequest, CalendarResponse>
{
    public override void Configure()
    {
        Put("/api/calendars/{id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RenameCalendarRequest req, CancellationToken ct)
    {
        var calendar = await req.ToCommand().ExecuteAsync(ct);

        await Send.OkAsync(CalendarResponse.From(calendar), ct);
    }
}
