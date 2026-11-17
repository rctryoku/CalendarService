# Calendar Service

A calendar API in C# / ASP.NET Core (.NET 10). Users have calendars with events. Events can repeat
daily or weekly and can book shared resources such as meeting rooms. A room can't be double-booked,
even from another person's calendar.


## Running it

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download). On Windows:

```powershell
tools\run.cmd            # restore, build, start the API and open Swagger in the browser
tools\run.cmd -Test      # run all tests first, stop if any fail
tools\run.cmd -Reset     # delete the local database first, start with no data
```

Or on any platform:

```bash
dotnet test
dotnet run --project src/Calendar.Api     # Swagger UI at http://localhost:5055/swagger
```

The API is at `http://localhost:5055`. Swagger groups the endpoints by feature (Calendars, Events,
Resources). The database is a SQLite file (`calendar.db`) that is created and migrated on startup.

**Walkthrough:** `src/Calendar.Api/Calendar.Api.http` has 17 numbered requests that go through every
feature, including a booking clash that returns 409. Send them from top to bottom with the
[REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension for
VS Code (recommended in `.vscode/extensions.json`). The last requests delete everything again, so the
file can be run more than once.

## The API

| Feature | Endpoints |
|---|---|
| Calendars | `POST/GET /api/calendars` (`?owner=` filter), `GET/PUT/DELETE /api/calendars/{id}` |
| Events | `POST/GET /api/calendars/{calendarId}/events` (`?from=&to=`), `GET/PUT/DELETE .../events/{id}` |
| Resources | `POST/GET /api/resources`, `GET/PUT/DELETE /api/resources/{id}` |

An event with a weekly recurrence that books a room:

```json
{
  "title": "Planning",
  "startDate": "2026-11-18T13:00:00+01:00",
  "endDate": "2026-11-18T14:00:00+01:00",
  "recurrence": { "frequency": 2, "interval": 1, "count": 10 },
  "resourceIds": ["<resource id>"]
}
```

- `frequency` is `1` (daily) or `2` (weekly). `interval: 2` means every other day or week. Without
  `count` the event repeats forever.
- Listing events needs a time range (`from`, `to`, at most 366 days) and returns **occurrences**: a
  weekly meeting appears once per week, each with the id of the event it belongs to.


## Architecture

```
src/
  Calendar.Core          Entities, domain logic (recurrence, booking conflicts), repository interfaces. No packages.
  Calendar.Application   Commands, queries and their handlers. Depends on Core only.
  Calendar.Data          Dapper + SQLite repositories, SQL migration scripts. Depends on Core.
  Calendar.Api           FastEndpoints endpoints, requests/responses, validation, wiring.
tests/
  Calendar.UnitTests         Recurrence, conflict detection and validation rules, no database or HTTP.
  Calendar.IntegrationTests  The full HTTP pipeline against an in-memory SQLite database per test.
```

The dependency rule is the usual clean-architecture one: Api → Application → Core and Data → Core.
The domain and use cases know nothing about HTTP, Dapper or SQLite.

Inside each project the code is grouped by **feature and use case**, with the same folder names
everywhere:

```
Calendar.Api/Features/Events/Create/          CreateEventEndpoint, CreateEventRequest, CreateEventValidator
Calendar.Application/Features/Events/Create/  CreateEventCommand + CreateEventCommandHandler
Calendar.Core/Features/Events/Create/         ICreateEventRepository
Calendar.Data/Features/Events/Create/         CreateEventRepository
```

A request flows the same way for every endpoint: the endpoint validates the request, maps it to a
command with `ToCommand()`, runs it on the FastEndpoints command bus, and maps the result to a
response. The handler holds the use case. The repository holds one SQL statement.

**Why these choices**

- **FastEndpoints** gives one class per endpoint, typed requests and responses, FluentValidation and
  a command bus out of the box. That's the structure I want, without MediatR or controllers that grow
  into service classes.
- **Dapper with hand-written SQL** keeps the queries visible and easy to review. The date and booking
  queries are the interesting part of this domain, so I didn't want an ORM to hide them.
- **Plain SQL migration scripts** (`Calendar.Data/Migrations/`) can be read, reviewed and run by hand
  in any SQLite tool. A small migrator runs pending scripts once each at startup. Migration `0003`
  shows a real schema change on existing data (events moved into a default calendar).
- **SQLite** means there is nothing to install. The schema uses `Strict` tables, foreign keys with
  cascading deletes, and stores times as fixed-width ISO 8601 text so text order is time order.

## Design decisions

- **Times are UTC, ranges are half-open.** The API accepts any offset and stores UTC. An event is
  `[StartDate, EndDate)`, so a meeting ending at 10:00 doesn't clash with one starting at 10:00.
- **Occurrences are calculated, not stored.** A recurring event is one row with its rule.
  `EventOccurrences.Between(event, from, to)` works out the occurrences in a range. It jumps straight
  to the first occurrence in the range instead of walking from the series start, so asking for 2036
  costs the same as asking for next week. Editing the series edits every occurrence, and an endless
  series takes no space. The cost is computing occurrences on every read, which is why listing needs
  a bounded range.
- **Each occurrence must end before the next one starts** (a 2-day event can't repeat daily). This
  keeps the occurrences of one series from overlapping each other.
- **Resources belong to no calendar.** A room is shared by everyone, which is what makes it "busy
  from another calendar".
- **Conflict detection is a pure function.** `BookingConflicts.FindFirst` takes the new event and the
  other bookings of the same resources (from any calendar) and checks every occurrence of the new
  event against them. A series with a count is checked to its last occurrence. A series without an
  end is checked one year ahead. The function has no database or time dependency, so it is unit
  tested directly, for single events and series in both directions.
- **Ids are Guid v7**, created by the handler, so they sort by creation time and can be generated
  without a database round trip.

## Tests

84 tests: 37 unit tests and 47 integration tests (`dotnet test`).

- **Unit tests** cover the logic that is easy to get wrong: recurrence expansion (intervals, counts,
  ranges far in the future, occurrences spanning the range start), booking conflicts, and the
  validation rules.
- **Integration tests** run the real API through `WebApplicationFactory`, each with its own in-memory
  database. They cover each endpoint's happy path and main error paths, including that an event can't
  be read or changed through another user's calendar and that a room booked by one user is busy for
  another.

Build warnings are errors, the .NET analysers are on, and `dotnet format --verify-no-changes` keeps
the style consistent.

## Known limitations

These are deliberate cuts for the time box, not oversights:

- **No authentication.** `owner` is a plain string and anyone can read or change any calendar.
- **Booking race.** Conflicts are checked before the event is saved, in separate database calls. Two
  requests for the same room at the same moment could both pass the check.
- **Recurrence is in UTC.** A weekly meeting at 10:00 Copenhagen time in winter shows as 11:00 in
  summer, because daylight saving time isn't taken into account.
- **Endless series are checked one year ahead, counted from the series start.** A clash further out,
  or a series that started long ago, isn't caught.
- **Duplicate names are allowed.** Two calendars called "Work" for the same owner, or two rooms with
  the same name, can only be told apart by their id.
- **No paging** on the list endpoints.

## What's next

### Product

- **Users and sharing.** Real sign-in, calendars owned by a user, and sharing a calendar with read or
  write access. In a care setting this maps to staff, residents and relatives seeing different things.
- **Attendees and invitations.** Invite people to an event, with accept/decline, so one event appears
  in several calendars instead of being copied.
- **Time zones.** Store the time zone with recurring events (e.g. `Europe/Copenhagen`) so a meeting
  stays at 10:00 local time all year.
- **Changing one occurrence.** Move or cancel a single occurrence of a series ("this one / this and
  following / all"), stored as exceptions to the rule.
- **Richer recurrence.** Monthly and yearly, several weekdays ("Mon, Wed, Fri"), an end date. Following
  iCalendar's RRULE keeps it compatible with other calendars.
- **Unique names.** A calendar name unique per owner and a room name unique, returning 409 on a
  duplicate, enforced by a unique index.
- **Smarter resources.** Room capacity and equipment, finding a free room for a given time, and
  showing a room's own calendar.
- **Reminders and sync.** Notifications before an event, and iCal export or Outlook/Google sync.

### Technical

- **Close the booking race.** Run the conflict check and the save in one transaction that serialises
  writers (`Begin Immediate` in SQLite, or `Serializable` or a lock per resource in PostgreSQL /
  SQL Server). A database exclusion constraint alone doesn't work, because occurrences aren't stored.
- **Check endless series from today.** Start the one-year window at the later of the series start
  and now (with an injected `TimeProvider`), or store the expanded bookings for a rolling window.
- **A production database.** PostgreSQL or SQL Server, with the same SQL scripts run by a migration
  tool such as DbUp, and connection pooling.
- **Authentication and authorisation.** JWT bearer tokens, the owner taken from the token instead of
  the request, and a check that the caller may access the calendar.
- **Paging and filtering** on the list endpoints, and API versioning before the first external client.
- **Observability.** Structured logging, OpenTelemetry traces and metrics, health checks.
- **Delivery.** A CI pipeline running build, tests and format checks on every pull request, and a
  container image.
- **Performance when it's needed.** If expanding occurrences on every read becomes expensive, cache
  the expansion per event or store occurrences for a rolling window. The current design keeps that
  decision open.
