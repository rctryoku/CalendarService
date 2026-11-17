using Calendar.Application.Features.Calendars.GetById;
using FastEndpoints;

namespace Calendar.Api.Features.Calendars.GetById;

public sealed class GetCalendarByIdEndpoint : Endpoint<GetCalendarByIdRequest, CalendarResponse>
{
    public override void Configure()
    {
        Get("/api/calendars/{id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetCalendarByIdRequest req, CancellationToken ct)
    {
        var calendar = await new GetCalendarByIdQuery(req.Id).ExecuteAsync(ct);

        await Send.OkAsync(CalendarResponse.From(calendar), ct);
    }
}
