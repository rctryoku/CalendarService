using System.Net;
using System.Net.Http.Json;
using Calendar.Api.Features.Calendars;
using Calendar.Api.Features.Calendars.Create;
using Calendar.Api.Features.Calendars.Rename;
using static Calendar.IntegrationTests.HttpClientExtensions;

namespace Calendar.IntegrationTests;

public sealed class CalendarsApiTests : IDisposable
{
    private readonly CalendarApiFactory _factory = new();
    private readonly HttpClient _client;

    public CalendarsApiTests() => _client = _factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Create_ValidCalendar_ReturnsLocationAndCanBeFetched()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/calendars", new CreateCalendarRequest { Name = "Work", Owner = "rene" });

        var created = await response.Content.ReadFromJsonAsync<CalendarResponse>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<CalendarResponse>(response.Headers.Location);
        Assert.Equal(created, fetched);
        Assert.Equal("Work", fetched!.Name);
        Assert.Equal("rene", fetched.Owner);
    }

    [Theory]
    [InlineData("", "rene")]
    [InlineData("Work", "")]
    public async Task Create_MissingNameOrOwner_Returns400(string name, string owner)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/calendars", new CreateCalendarRequest { Name = name, Owner = owner });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_WithOwner_ReturnsOnlyThatOwnersCalendarsSortedByName()
    {
        await _client.CreateCalendarAsync("Work", "rene");
        await _client.CreateCalendarAsync("Family", "rene");
        await _client.CreateCalendarAsync("Work", "claus");

        var calendars = await _client.GetFromJsonAsync<List<CalendarResponse>>("/api/calendars?owner=rene");

        Assert.Equal(["Family", "Work"], calendars!.Select(c => c.Name));
    }

    [Fact]
    public async Task List_WithoutOwner_ReturnsAllCalendars()
    {
        await _client.CreateCalendarAsync("Work", "rene");
        await _client.CreateCalendarAsync("Work", "claus");

        var calendars = await _client.GetFromJsonAsync<List<CalendarResponse>>("/api/calendars");

        Assert.Equal(2, calendars!.Count);
    }

    [Fact]
    public async Task Rename_ExistingCalendar_ChangesNameAndKeepsOwner()
    {
        var calendar = await _client.CreateCalendarAsync("Work", "rene");

        var response = await _client.PutAsJsonAsync(
            $"/api/calendars/{calendar.Id}", new RenameCalendarRequest { Name = "Job" });

        var renamed = await response.Content.ReadFromJsonAsync<CalendarResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new CalendarResponse(calendar.Id, "Job", "rene"), renamed);
    }

    [Fact]
    public async Task Delete_CalendarWithEvents_DeletesItsEvents()
    {
        var calendar = await _client.CreateCalendarAsync();
        var calendarEvent = await _client.CreateEventAsync(
            calendar.Id, EventRequest("Standup", "2026-11-18T09:00:00Z", "2026-11-18T09:15:00Z"));

        var delete = await _client.DeleteAsync($"/api/calendars/{calendar.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/calendars/{calendar.Id}")).StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await _client.GetAsync($"/api/calendars/{calendar.Id}/events/{calendarEvent.Id}")).StatusCode);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task AnyMethod_UnknownId_Returns404(string method)
    {
        var url = $"/api/calendars/{Guid.NewGuid()}";

        var response = method switch
        {
            "GET" => await _client.GetAsync(url),
            "PUT" => await _client.PutAsJsonAsync(url, new RenameCalendarRequest { Name = "Job" }),
            _ => await _client.DeleteAsync(url),
        };

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
