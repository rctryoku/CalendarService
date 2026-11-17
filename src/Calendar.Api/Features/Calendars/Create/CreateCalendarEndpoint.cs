using Calendar.Api.Features.Calendars.GetById;
using FastEndpoints;

namespace Calendar.Api.Features.Calendars.Create;

public sealed class CreateCalendarEndpoint : Endpoint<CreateCalendarRequest, CalendarResponse>
{
    public override void Configure()
    {
        Post("/");
        Group<CalendarsGroup>();
    }

    public override async Task HandleAsync(CreateCalendarRequest req, CancellationToken ct)
    {
        var calendar = await req.ToCommand().ExecuteAsync(ct);

        await Send.CreatedAtAsync<GetCalendarByIdEndpoint>(
            new { id = calendar.Id }, CalendarResponse.From(calendar), cancellation: ct);
    }
}
