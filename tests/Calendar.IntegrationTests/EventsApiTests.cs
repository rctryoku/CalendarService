using System.Net;
using System.Net.Http.Json;
using Calendar.Api.Features.Calendars;
using Calendar.Api.Features.Events;
using Calendar.Api.Features.Events.Create;
using Calendar.Api.Features.Events.List;
using Calendar.Core.Features.Events.Recurrence;
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
    public async Task Create_ValidEvent_ReturnsLocationAndStoresTimesInUtc()
    {
        var response = await _client.PostAsJsonAsync(
            EventsUrl, EventRequest("Standup", "2026-11-18T09:00:00+01:00", "2026-11-18T09:15:00+01:00"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<EventResponse>(response.Headers.Location);
        Assert.Equal("Standup", fetched!.Title);
        Assert.Equal(_calendar.Id, fetched.CalendarId);
        Assert.Equal(new DateTime(2026, 11, 18, 8, 0, 0, DateTimeKind.Utc), fetched.StartDate);
        Assert.Equal(new DateTime(2026, 11, 18, 8, 15, 0, DateTimeKind.Utc), fetched.EndDate);
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

        var events = await ListAsync("2026-11-18T00:00:00Z", "2026-11-19T00:00:00Z");

        Assert.Equal(["Spans midnight", "Late"], events.Select(e => e.Title));
    }

    [Fact]
    public async Task List_OnlyReturnsEventsFromThatCalendar()
    {
        var otherCalendar = await _client.CreateCalendarAsync("Private", "someone-else");
        await CreateAsync(EventRequest("Mine", "2026-11-18T09:00:00Z", "2026-11-18T10:00:00Z"));
        await _client.CreateEventAsync(
            otherCalendar.Id, EventRequest("Theirs", "2026-11-18T09:00:00Z", "2026-11-18T10:00:00Z"));

        var events = await ListAsync("2026-11-18T00:00:00Z", "2026-11-19T00:00:00Z");

        Assert.Equal(["Mine"], events.Select(e => e.Title));
    }

    [Fact]
    public async Task List_UnknownCalendar_Returns404()
    {
        var response = await _client.GetAsync(
            $"/api/calendars/{Guid.NewGuid()}/events?from=2026-11-18T00:00:00Z&to=2026-11-19T00:00:00Z");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("?from=2026-11-18T00:00:00Z")]
    [InlineData("?from=2026-11-19T00:00:00Z&to=2026-11-18T00:00:00Z")]
    [InlineData("?from=2026-01-01T00:00:00Z&to=2027-06-01T00:00:00Z")]
    public async Task List_MissingOrInvalidRange_Returns400(string query)
    {
        var response = await _client.GetAsync($"{EventsUrl}{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_RecurringEvent_IsReturnedWithItsRule()
    {
        var created = await CreateAsync(EventRequest("Standup", "2026-11-16T09:00:00Z", "2026-11-16T09:15:00Z") with
        {
            Recurrence = new RecurrenceRequest { Frequency = RecurrenceFrequency.Weekly, Interval = 2, Count = 5 },
        });

        var fetched = await _client.GetFromJsonAsync<EventResponse>($"{EventsUrl}/{created.Id}");

        Assert.Equal(new RecurrenceResponse(RecurrenceFrequency.Weekly, 2, 5), fetched!.Recurrence);
    }

    [Fact]
    public async Task List_DailyEvent_ReturnsOneOccurrencePerDayMixedWithSingleEvents()
    {
        await CreateAsync(EventRequest("Standup", "2026-11-16T09:00:00Z", "2026-11-16T09:15:00Z") with
        {
            Recurrence = new RecurrenceRequest { Frequency = RecurrenceFrequency.Daily },
        });
        await CreateAsync(EventRequest("Lunch", "2026-11-17T12:00:00Z", "2026-11-17T13:00:00Z"));

        var occurrences = await ListAsync("2026-11-16T00:00:00Z", "2026-11-19T00:00:00Z");

        Assert.Equal(["Standup", "Standup", "Lunch", "Standup"], occurrences.Select(o => o.Title));
        Assert.Equal(
            new DateTime(2026, 11, 18, 9, 0, 0, DateTimeKind.Utc),
            occurrences.Last().StartDate);
        Assert.True(occurrences.Last().IsRecurring);
    }

    [Fact]
    public async Task Update_RemovingRecurrence_LeavesSingleEvent()
    {
        var created = await CreateAsync(EventRequest("Standup", "2026-11-16T09:00:00Z", "2026-11-16T09:15:00Z") with
        {
            Recurrence = new RecurrenceRequest { Frequency = RecurrenceFrequency.Daily },
        });

        await _client.PutAsJsonAsync(
            $"{EventsUrl}/{created.Id}", EventRequest("Standup", "2026-11-16T09:00:00Z", "2026-11-16T09:15:00Z"));

        var occurrences = await ListAsync("2026-11-16T00:00:00Z", "2026-11-23T00:00:00Z");
        var occurrence = Assert.Single(occurrences);
        Assert.False(occurrence.IsRecurring);
    }

    private async Task<List<EventOccurrenceResponse>> ListAsync(string from, string to) =>
        (await _client.GetFromJsonAsync<List<EventOccurrenceResponse>>($"{EventsUrl}?from={from}&to={to}"))!;

    private Task<EventResponse> CreateAsync(CreateEventRequest request) =>
        _client.CreateEventAsync(_calendar.Id, request);
}
