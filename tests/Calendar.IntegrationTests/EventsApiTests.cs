using System.Net;
using System.Net.Http.Json;
using Calendar.Api.Features.Calendars;
using Calendar.Api.Features.Events;
using Calendar.Api.Features.Events.Create;
using static Calendar.IntegrationTests.HttpClientExtensions;

namespace Calendar.IntegrationTests;

public sealed class EventsApiTests : IAsyncLifetime, IDisposable
{
    private readonly CalendarApiFactory _factory = new();
    private readonly HttpClient _client;
    private CalendarResponse _calendar = null!;

    public EventsApiTests() => _client = _factory.CreateClient();

    private string EventsUrl => $"/api/calendars/{_calendar.Id}/events";

    public async Task InitializeAsync() => _calendar = await _client.CreateCalendarAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Create_ValidEvent_StoresTimesInUtc()
    {
        var created = await CreateAsync(EventRequest("Standup", "2026-11-18T09:00:00+01:00", "2026-11-18T09:15:00+01:00"));

        var fetched = await _client.GetFromJsonAsync<EventResponse>($"{EventsUrl}/{created.Id}");

        Assert.NotNull(fetched);
        Assert.Equal("Standup", fetched.Title);
        Assert.Equal(_calendar.Id, fetched.CalendarId);
        Assert.Equal(new DateTime(2026, 11, 18, 8, 0, 0, DateTimeKind.Utc), fetched.StartDate);
        Assert.Equal(new DateTime(2026, 11, 18, 8, 15, 0, DateTimeKind.Utc), fetched.EndDate);
    }

    [Fact]
    public async Task Create_ValidEvent_ReturnsLocationOfNewEvent()
    {
        var response = await _client.PostAsJsonAsync(
            EventsUrl, EventRequest("Standup", "2026-11-18T09:00:00Z", "2026-11-18T09:15:00Z"));

        var created = await response.Content.ReadFromJsonAsync<EventResponse>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith(
            $"{EventsUrl}/{created!.Id}", response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_UnknownCalendar_Returns404()
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/calendars/{Guid.NewGuid()}/events",
            EventRequest("Standup", "2026-11-18T09:00:00Z", "2026-11-18T09:15:00Z"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_EndBeforeStart_Returns400WithReason()
    {
        var response = await _client.PostAsJsonAsync(
            EventsUrl, EventRequest("Backwards", "2026-11-18T10:00:00Z", "2026-11-18T09:00:00Z"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("EndDate must be after StartDate.", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Create_EmptyTitle_Returns400()
    {
        var response = await _client.PostAsJsonAsync(
            EventsUrl, EventRequest("", "2026-11-18T09:00:00Z", "2026-11-18T10:00:00Z"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingEvent_ChangesEvent()
    {
        var created = await CreateAsync(EventRequest("Standup", "2026-11-18T09:00:00Z", "2026-11-18T09:15:00Z"));

        var response = await _client.PutAsJsonAsync(
            $"{EventsUrl}/{created.Id}",
            EventRequest("Standup (moved)", "2026-11-18T10:00:00Z", "2026-11-18T10:15:00Z"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<EventResponse>($"{EventsUrl}/{created.Id}");
        Assert.Equal("Standup (moved)", fetched!.Title);
        Assert.Equal(new DateTime(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc), fetched.StartDate);
    }

    [Fact]
    public async Task Delete_ExistingEvent_RemovesEvent()
    {
        var created = await CreateAsync(EventRequest("Standup", "2026-11-18T09:00:00Z", "2026-11-18T09:15:00Z"));

        var delete = await _client.DeleteAsync($"{EventsUrl}/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        var get = await _client.GetAsync($"{EventsUrl}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task AnyMethod_UnknownId_Returns404(string method)
    {
        var url = $"{EventsUrl}/{Guid.NewGuid()}";

        var response = method switch
        {
            "GET" => await _client.GetAsync(url),
            "PUT" => await _client.PutAsJsonAsync(url, EventRequest("X", "2026-11-18T09:00:00Z", "2026-11-18T10:00:00Z")),
            _ => await _client.DeleteAsync(url),
        };

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task AnyMethod_EventFromOtherCalendar_Returns404AndLeavesEventUnchanged(string method)
    {
        var otherCalendar = await _client.CreateCalendarAsync("Private", "someone-else");
        var otherEvent = await _client.CreateEventAsync(
            otherCalendar.Id, EventRequest("Dentist", "2026-11-18T09:00:00Z", "2026-11-18T10:00:00Z"));
        var urlThroughWrongCalendar = $"{EventsUrl}/{otherEvent.Id}";

        var response = method switch
        {
            "GET" => await _client.GetAsync(urlThroughWrongCalendar),
            "PUT" => await _client.PutAsJsonAsync(
                urlThroughWrongCalendar, EventRequest("Hijacked", "2026-11-18T09:00:00Z", "2026-11-18T10:00:00Z")),
            _ => await _client.DeleteAsync(urlThroughWrongCalendar),
        };

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var unchanged = await _client.GetFromJsonAsync<EventResponse>(
            $"/api/calendars/{otherCalendar.Id}/events/{otherEvent.Id}");
        Assert.Equal("Dentist", unchanged!.Title);
    }

    [Fact]
    public async Task List_WithRange_ReturnsOverlappingEventsSortedByStart()
    {
        await CreateAsync(EventRequest("Before", "2026-11-17T09:00:00Z", "2026-11-17T10:00:00Z"));
        await CreateAsync(EventRequest("Late", "2026-11-18T15:00:00Z", "2026-11-18T16:00:00Z"));
        await CreateAsync(EventRequest("Spans midnight", "2026-11-17T23:00:00Z", "2026-11-18T01:00:00Z"));
        await CreateAsync(EventRequest("After", "2026-11-19T09:00:00Z", "2026-11-19T10:00:00Z"));

        var events = await _client.GetFromJsonAsync<List<EventResponse>>(
            $"{EventsUrl}?from=2026-11-18T00:00:00Z&to=2026-11-19T00:00:00Z");

        Assert.Equal(["Spans midnight", "Late"], events!.Select(e => e.Title));
    }

    [Fact]
    public async Task List_OnlyReturnsEventsFromThatCalendar()
    {
        var otherCalendar = await _client.CreateCalendarAsync("Private", "someone-else");
        await CreateAsync(EventRequest("Mine", "2026-11-18T09:00:00Z", "2026-11-18T10:00:00Z"));
        await _client.CreateEventAsync(
            otherCalendar.Id, EventRequest("Theirs", "2026-11-18T09:00:00Z", "2026-11-18T10:00:00Z"));

        var events = await _client.GetFromJsonAsync<List<EventResponse>>(EventsUrl);

        Assert.Equal(["Mine"], events!.Select(e => e.Title));
    }

    [Fact]
    public async Task List_UnknownCalendar_Returns404()
    {
        var response = await _client.GetAsync($"/api/calendars/{Guid.NewGuid()}/events");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private Task<EventResponse> CreateAsync(CreateEventRequest request) =>
        _client.CreateEventAsync(_calendar.Id, request);
}
