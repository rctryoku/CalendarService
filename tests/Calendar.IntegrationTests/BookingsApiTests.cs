using System.Net;
using System.Net.Http.Json;
using Calendar.Api.Features.Calendars;
using Calendar.Api.Features.Events;
using Calendar.Api.Features.Events.Create;
using Calendar.Api.Features.Resources;
using Calendar.Core.Features.Events.Recurrence;
using Microsoft.AspNetCore.Mvc;
using static Calendar.IntegrationTests.HttpClientExtensions;

namespace Calendar.IntegrationTests;

public sealed class BookingsApiTests : IAsyncLifetime, IDisposable
{
    private readonly CalendarApiFactory _factory = new();
    private readonly HttpClient _client;
    private CalendarResponse _alice = null!;
    private CalendarResponse _bob = null!;
    private ResourceResponse _room = null!;

    public BookingsApiTests() => _client = _factory.CreateClient();

    public async Task InitializeAsync()
    {
        _alice = await _client.CreateCalendarAsync("Work", "alice");
        _bob = await _client.CreateCalendarAsync("Work", "bob");
        _room = await _client.CreateResourceAsync("Room 1");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Create_EventBookingResource_StoresTheBooking()
    {
        var created = await _client.CreateEventAsync(_alice.Id, Booking("Planning", "09:00", "10:00"));

        var fetched = await _client.GetFromJsonAsync<EventResponse>($"/api/calendars/{_alice.Id}/events/{created.Id}");

        Assert.Equal([_room.Id], fetched!.ResourceIds);
    }

    [Fact]
    public async Task Create_ResourceBookedInOtherCalendar_Returns409WithReason()
    {
        await _client.CreateEventAsync(_alice.Id, Booking("Planning", "09:00", "10:00"));

        var response = await PostAsync(_bob.Id, Booking("Interview", "09:30", "10:30"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("'Room 1' is already booked by 'Planning' at 2026-11-18 09:00:00Z.", problem!.Detail);
    }

    [Fact]
    public async Task Create_RecurringBookingHittingLaterEvent_Returns409()
    {
        await _client.CreateEventAsync(
            _bob.Id, EventRequest("Workshop", "2026-12-02T09:00:00Z", "2026-12-02T12:00:00Z") with { ResourceIds = [_room.Id] });

        var response = await PostAsync(
            _alice.Id,
            Booking("Team meeting", "09:00", "10:00") with { Recurrence = new() { Frequency = RecurrenceFrequency.Weekly } });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_OverlappingEventsWithoutResources_Returns201()
    {
        await _client.CreateEventAsync(_alice.Id, Booking("Planning", "09:00", "10:00") with { ResourceIds = [] });

        var response = await PostAsync(_bob.Id, Booking("Interview", "09:00", "10:00") with { ResourceIds = [] });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_UnknownResource_Returns404()
    {
        var response = await PostAsync(_alice.Id, Booking("Planning", "09:00", "10:00") with { ResourceIds = [Guid.NewGuid()] });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_MovingEventWithinItsOwnBooking_Returns200()
    {
        var created = await _client.CreateEventAsync(_alice.Id, Booking("Planning", "09:00", "10:00"));

        var response = await _client.PutAsJsonAsync(
            $"/api/calendars/{_alice.Id}/events/{created.Id}", Booking("Planning", "09:30", "10:30"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_MovingEventOntoOtherBooking_Returns409()
    {
        await _client.CreateEventAsync(_alice.Id, Booking("Planning", "09:00", "10:00"));
        var other = await _client.CreateEventAsync(_bob.Id, Booking("Interview", "13:00", "14:00"));

        var response = await _client.PutAsJsonAsync(
            $"/api/calendars/{_bob.Id}/events/{other.Id}", Booking("Interview", "09:00", "10:00"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_BookedEvent_FreesTheResource()
    {
        var created = await _client.CreateEventAsync(_alice.Id, Booking("Planning", "09:00", "10:00"));
        await _client.DeleteAsync($"/api/calendars/{_alice.Id}/events/{created.Id}");

        var response = await PostAsync(_bob.Id, Booking("Interview", "09:00", "10:00"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private CreateEventRequest Booking(string title, string startTime, string endTime) =>
        EventRequest(title, $"2026-11-18T{startTime}:00Z", $"2026-11-18T{endTime}:00Z") with { ResourceIds = [_room.Id] };

    private Task<HttpResponseMessage> PostAsync(Guid calendarId, CreateEventRequest request) =>
        _client.PostAsJsonAsync($"/api/calendars/{calendarId}/events", request);
}
