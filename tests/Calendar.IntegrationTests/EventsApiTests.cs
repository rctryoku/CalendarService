using System.Net;
using System.Net.Http.Json;
using Calendar.Api.Features.Events;
using Calendar.Api.Features.Events.Create;

namespace Calendar.IntegrationTests;

public sealed class EventsApiTests : IDisposable
{
    private readonly CalendarApiFactory _factory = new();
    private readonly HttpClient _client;

    public EventsApiTests() => _client = _factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Create_ValidEvent_StoresTimesInUtc()
    {
        var created = await CreateAsync(Request("Standup", "2026-11-18T09:00:00+01:00", "2026-11-18T09:15:00+01:00"));

        var fetched = await _client.GetFromJsonAsync<EventResponse>($"/api/events/{created.Id}");

        Assert.NotNull(fetched);
        Assert.Equal("Standup", fetched.Title);
        Assert.Equal(new DateTime(2026, 11, 18, 8, 0, 0, DateTimeKind.Utc), fetched.StartDate);
        Assert.Equal(new DateTime(2026, 11, 18, 8, 15, 0, DateTimeKind.Utc), fetched.EndDate);
    }

    [Fact]
    public async Task Create_ValidEvent_ReturnsLocationOfNewEvent()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/events", Request("Standup", "2026-11-18T09:00:00Z", "2026-11-18T09:15:00Z"));

        var created = await response.Content.ReadFromJsonAsync<EventResponse>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith(
            $"/api/events/{created!.Id}", response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_EndBeforeStart_Returns400WithReason()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/events", Request("Backwards", "2026-11-18T10:00:00Z", "2026-11-18T09:00:00Z"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("EndDate must be after StartDate.", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Create_EmptyTitle_Returns400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/events", Request("", "2026-11-18T09:00:00Z", "2026-11-18T10:00:00Z"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingEvent_ChangesEvent()
    {
        var created = await CreateAsync(Request("Standup", "2026-11-18T09:00:00Z", "2026-11-18T09:15:00Z"));

        var response = await _client.PutAsJsonAsync(
            $"/api/events/{created.Id}", Request("Standup (moved)", "2026-11-18T10:00:00Z", "2026-11-18T10:15:00Z"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<EventResponse>($"/api/events/{created.Id}");
        Assert.Equal("Standup (moved)", fetched!.Title);
        Assert.Equal(new DateTime(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc), fetched.StartDate);
    }

    [Fact]
    public async Task Delete_ExistingEvent_RemovesEvent()
    {
        var created = await CreateAsync(Request("Standup", "2026-11-18T09:00:00Z", "2026-11-18T09:15:00Z"));

        var delete = await _client.DeleteAsync($"/api/events/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        var get = await _client.GetAsync($"/api/events/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task AnyMethod_UnknownId_Returns404(string method)
    {
        var url = $"/api/events/{Guid.NewGuid()}";

        var response = method switch
        {
            "GET" => await _client.GetAsync(url),
            "PUT" => await _client.PutAsJsonAsync(url, Request("X", "2026-11-18T09:00:00Z", "2026-11-18T10:00:00Z")),
            _ => await _client.DeleteAsync(url),
        };

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_WithRange_ReturnsOverlappingEventsSortedByStart()
    {
        await CreateAsync(Request("Before", "2026-11-17T09:00:00Z", "2026-11-17T10:00:00Z"));
        await CreateAsync(Request("Late", "2026-11-18T15:00:00Z", "2026-11-18T16:00:00Z"));
        await CreateAsync(Request("Spans midnight", "2026-11-17T23:00:00Z", "2026-11-18T01:00:00Z"));
        await CreateAsync(Request("After", "2026-11-19T09:00:00Z", "2026-11-19T10:00:00Z"));

        var events = await _client.GetFromJsonAsync<List<EventResponse>>(
            "/api/events?from=2026-11-18T00:00:00Z&to=2026-11-19T00:00:00Z");

        Assert.Equal(["Spans midnight", "Late"], events!.Select(e => e.Title));
    }

    private static CreateEventRequest Request(string title, string startDate, string endDate) => new()
    {
        Title = title,
        StartDate = DateTimeOffset.Parse(startDate, System.Globalization.CultureInfo.InvariantCulture),
        EndDate = DateTimeOffset.Parse(endDate, System.Globalization.CultureInfo.InvariantCulture),
    };

    private async Task<EventResponse> CreateAsync(CreateEventRequest request)
    {
        var response = await _client.PostAsJsonAsync("/api/events", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<EventResponse>())!;
    }
}
