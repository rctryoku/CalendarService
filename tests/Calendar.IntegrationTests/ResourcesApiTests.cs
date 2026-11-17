using System.Net;
using System.Net.Http.Json;
using Calendar.Api.Features.Events;
using Calendar.Api.Features.Resources;
using Calendar.Api.Features.Resources.Create;
using Calendar.Api.Features.Resources.Rename;
using static Calendar.IntegrationTests.HttpClientExtensions;

namespace Calendar.IntegrationTests;

public sealed class ResourcesApiTests : IDisposable
{
    private readonly CalendarApiFactory _factory = new();
    private readonly HttpClient _client;

    public ResourcesApiTests() => _client = _factory.CreateClient();

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Create_ValidResource_ReturnsLocationAndCanBeFetched()
    {
        var response = await _client.PostAsJsonAsync("/api/resources", new CreateResourceRequest { Name = "Room 1" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<ResourceResponse>(response.Headers.Location);
        Assert.Equal("Room 1", fetched!.Name);
    }

    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/resources", new CreateResourceRequest { Name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsResourcesOrderedByName()
    {
        await _client.CreateResourceAsync("Room B");
        await _client.CreateResourceAsync("Room A");

        var resources = await _client.GetFromJsonAsync<List<ResourceResponse>>("/api/resources");

        Assert.Equal(["Room A", "Room B"], resources!.Select(resource => resource.Name));
    }

    [Fact]
    public async Task Rename_ExistingResource_ReturnsNewName()
    {
        var resource = await _client.CreateResourceAsync("Room 1");

        var response = await _client.PutAsJsonAsync(
            $"/api/resources/{resource.Id}", new RenameResourceRequest { Name = "Board room" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Board room", (await response.Content.ReadFromJsonAsync<ResourceResponse>())!.Name);
    }

    [Fact]
    public async Task Delete_BookedResource_RemovesItFromEvents()
    {
        var calendar = await _client.CreateCalendarAsync();
        var resource = await _client.CreateResourceAsync();
        var calendarEvent = await _client.CreateEventAsync(
            calendar.Id,
            EventRequest("Planning", "2026-11-18T09:00:00Z", "2026-11-18T10:00:00Z") with { ResourceIds = [resource.Id] });

        var delete = await _client.DeleteAsync($"/api/resources/{resource.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        var fetched = await _client.GetFromJsonAsync<EventResponse>(
            $"/api/calendars/{calendar.Id}/events/{calendarEvent.Id}");
        Assert.Empty(fetched!.ResourceIds);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task AnyMethod_UnknownId_Returns404(string method)
    {
        var url = $"/api/resources/{Guid.NewGuid()}";

        var response = method switch
        {
            "GET" => await _client.GetAsync(url),
            "PUT" => await _client.PutAsJsonAsync(url, new RenameResourceRequest { Name = "X" }),
            _ => await _client.DeleteAsync(url),
        };

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
