using FastEndpoints;

namespace Calendar.Api.Features.Calendars.Delete;

public sealed class DeleteCalendarEndpoint : Endpoint<DeleteCalendarRequest>
{
    public override void Configure()
    {
        Delete("/{id:guid}");
        Group<CalendarsGroup>();
    }

    public override async Task HandleAsync(DeleteCalendarRequest req, CancellationToken ct)
    {
        await req.ToCommand().ExecuteAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
