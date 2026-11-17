using FastEndpoints;

namespace Calendar.Api.Features.Events;

public sealed class EventsGroup : Group
{
    public EventsGroup() =>
        Configure("api/calendars/{calendarId:guid}/events", endpoint =>
        {
            endpoint.AllowAnonymous();
            endpoint.Description(builder => builder.WithTags("Events"));
        });
}
