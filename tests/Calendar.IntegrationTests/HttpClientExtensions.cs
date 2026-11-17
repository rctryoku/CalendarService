using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Calendar.Api.Features.Calendars;
using Calendar.Api.Features.Calendars.Create;
using Calendar.Api.Features.Events;
using Calendar.Api.Features.Events.Create;

namespace Calendar.IntegrationTests;

internal static class HttpClientExtensions
{
    public static async Task<CalendarResponse> CreateCalendarAsync(
        this HttpClient client, string name = "Work", string owner = "rene")
    {
        var response = await client.PostAsJsonAsync(
            "/api/calendars", new CreateCalendarRequest { Name = name, Owner = owner });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CalendarResponse>())!;
    }

    public static async Task<EventResponse> CreateEventAsync(
        this HttpClient client, Guid calendarId, CreateEventRequest request)
    {
        var response = await client.PostAsJsonAsync($"/api/calendars/{calendarId}/events", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<EventResponse>())!;
    }

    public static CreateEventRequest EventRequest(string title, string startDate, string endDate) => new()
    {
        Title = title,
        StartDate = DateTimeOffset.Parse(startDate, CultureInfo.InvariantCulture),
        EndDate = DateTimeOffset.Parse(endDate, CultureInfo.InvariantCulture),
    };
}
