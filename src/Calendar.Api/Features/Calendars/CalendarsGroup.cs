using FastEndpoints;

namespace Calendar.Api.Features.Calendars;

public sealed class CalendarsGroup : Group
{
    public CalendarsGroup() =>
        Configure("api/calendars", endpoint =>
        {
            endpoint.AllowAnonymous();
            endpoint.Description(builder => builder.WithTags("Calendars"));
        });
}
