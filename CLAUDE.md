# CLAUDE.md

A calendar API in C# / ASP.NET Core: calendars, events, recurrence and bookable resources.
Prefer a small, well-reasoned solution over breadth; every design decision should be easy to explain.

What we are building, the plan and the current status live in `PROGRESS.md` (local only, not in git).
Read it first when it exists.

## Commands

```powershell
tools\run.cmd                                 # restore, build, run, open Swagger (add -Test, -Reset)
dotnet build                                  # warnings are errors
dotnet test                                   # all tests
dotnet format --verify-no-changes             # style check (must pass)
dotnet run --project src/Calendar.Api         # Swagger UI at http://localhost:5055/swagger
```

Before calling any change done: `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes` must all pass.

## Principles

1. **Clean Code (Uncle Bob).** Small methods that do one thing, intention-revealing names, no flag
   arguments, no hidden side effects, no duplication. Code should read like well-written prose.
2. **Comments only when the code cannot say it.** If a comment is needed to explain *what* code does,
   rename or extract until it isn't. The only acceptable comments explain a non-obvious *why*
   (a trade-off, a library quirk, a business rule). XML docs only where they add information
   the signature doesn't.
3. **Golden path.** Guard clauses and early returns/throws first; the happy path reads straight
   down the left margin. No `else` after `return`/`throw`, no deep nesting.
4. **One thing per class.** One endpoint per class, one command handler per command. No "service"
   classes that collect many operations.
5. **Vertical slices inside clean layers.** Projects give the layers and the dependency rule;
   inside each project, code is grouped by feature (`Features/<Feature>/<UseCase>/`).

## Architecture

```
src/
  Calendar.Core          Entities, domain rules, exceptions, repository interfaces. No packages at all.
  Calendar.Application   Commands/queries + their handlers. Core + FastEndpoints.Messaging.Core only.
  Calendar.Data          Dapper + SQLite: repositories, schema, type handlers. Depends on Core.
  Calendar.Api           FastEndpoints endpoints, Request/Response types, validators, wiring. Composition root.
tests/
  Calendar.UnitTests         Core and Application logic, no database or HTTP.
  Calendar.IntegrationTests  Full HTTP pipeline via WebApplicationFactory + in-memory SQLite.
```

Dependency rule: Api → Application → Core, Api → Data → Core. Core references nothing.
Core and Application must never reference ASP.NET Core, Dapper, SQLite or any web/infrastructure package.

Feature layout (one use case per folder):

```
Calendar.Api/Features/Events/Create/CreateEventEndpoint.cs
Calendar.Api/Features/Events/Create/CreateEventRequest.cs
Calendar.Api/Features/Events/Create/CreateEventValidator.cs          (includes EventDetailsValidator)
Calendar.Api/Features/Events/EventResponse.cs                        (shared by the feature's endpoints)
Calendar.Api/Features/Events/EventDetailsValidator.cs                (rules shared by Create and Update)
Calendar.Application/Features/Events/Create/CreateEventCommandHandler.cs  (command record + handler)
Calendar.Core/Features/Events/CalendarEvent.cs
Calendar.Core/Features/Events/Create/ICreateEventRepository.cs
Calendar.Data/Features/Events/Create/CreateEventRepository.cs
```

Every use case has the same folder name (`Create`, `Update`, `Delete`, `GetById`, `List`, or an
intention-revealing verb such as `Rename`) in every project.

Features so far: `Calendars` (entity `UserCalendar`), `Resources` (entity `Resource`) and `Events` (entity `CalendarEvent`, always inside a
calendar: routes are `/api/calendars/{calendarId}/events[/{id}]`). The entity is not called `Calendar`
because that would clash with the root namespace `Calendar`.

Recurrence (`Core/Features/Events/Recurrence/`): an event has an optional `RecurrenceRule`
(Frequency `1 = Daily` / `2 = Weekly`, Interval, optional Count; no Count repeats forever). Occurrences are never stored; `EventOccurrences.Between(event, from, to)`
expands them at query time, so listing events requires `from` and `to` (max 366 days) and returns
occurrences (`EventOccurrenceResponse`). Anything that asks "is this busy?" must go through
`EventOccurrences`, never through `StartDate`/`EndDate` alone.

Resources (`Resource`, e.g. meeting rooms) are global, not owned by a calendar (`/api/resources`). An event
books them through `ResourceIds` (link table `EventResources`). Create and Update event call
`EnsureResourcesAreFreeAsync`, which loads other bookings of the same resources in **any** calendar
(`IResourceBookingRepository`) and runs the pure `BookingConflicts.FindFirst`; a clash throws
`ConflictException` (409). The check covers the event's whole series, or `BookingConflicts.OpenEndedHorizon`
(366 days) for a series without a count.

### Layer rules

- **Core**: entities are plain positional `record`s (`CalendarEvent(Guid Id, string Title, ...)`) with no
  behaviour, plus their length-limit constants. One small repository interface per use case
  (`ICreateEventRepository`, `IGetEventByIdRepository`, ...) in the use case's folder (interface segregation).
- **Application**: the command pattern via FastEndpoints' command bus. Writes are `XxxCommand`, reads are
  `XxxQuery`; both implement `ICommand<TResult>`. The command/query record lives **in the same file as its
  handler** (`XxxCommandHandler.cs` / `XxxQueryHandler.cs`), since one can't exist without the other.
  Handlers create entities (`Guid.CreateVersion7()`) and return them; mapping to responses happens in the Api.
  Missing entities throw `NotFoundException`, booking clashes throw `ConflictException`.
- **Data**: Dapper with hand-written SQL. One repository class per use case, mirroring Core's interfaces,
  a connection per operation via `SqliteConnectionFactory`. SQL is a `var sql = """..."""` local in the method.
  Column names match the C# property names, so no aliases are needed. When the table shape differs from the
  entity (e.g. the recurrence rule flattened into nullable columns), map through an internal row class
  (`EventRow` with `From(entity)` / `ToCalendarEvent()`) instead of fighting Dapper's constructor mapping.
  Enums have explicit values starting at 1 (so a missing value, 0, is invalid). They are stored and sent
  as those numbers: `Integer` column + `Check (... In (1, 2))`, plain numbers in JSON.
  Exposes `AddData(connectionString)` and `MigrateDatabaseAsync()`.

### Database migrations

- Every schema change (create/alter table, index) is a plain SQL file in `src/Calendar.Data/Migrations/`,
  named `NNNN_Description.sql` (e.g. `0002_CreateCalendarsTable.sql`). Never put DDL in C#.
- Scripts must be runnable as-is in any SQLite tool (copy, paste, run). No templating, no C# placeholders.
- Never edit a script that has been applied; add a new one. SQLite can't add a required or foreign key column
  with `Alter Table`, so such changes rebuild the table (see `0003_AddCalendarIdToEvents.sql`).
- Foreign keys are switched on per connection in `SqliteConnectionFactory`; rely on `On Delete Cascade`
  rather than deleting child rows by hand. `DatabaseMigrator` runs pending scripts in file
  name order at startup, each in a transaction, and records them in `SchemaVersions`.

### SQL style

Keywords in title case, each clause on its own line, one column per line, indented four spaces.
Queries live as `const string sql = """..."""` inside the repository method that uses them.

```sql
Select
    Id,
    Title,
    StartDate,
    EndDate
From
    Events
Where
    Id = @Id
Order By
    StartDate
```

Parameters are PascalCase and match property names (`@Id`, `@StartDate`); pass `new { Id = id }`.
- **Api**: one FastEndpoints endpoint class per route. Every feature has a `XxxGroup : Group` (e.g.
  `EventsGroup`) holding its route prefix, `AllowAnonymous()` and Swagger tag; endpoints call
  `Group<XxxGroup>()` and only declare the rest of the route (`Get("/{id:guid}")`). Swagger groups by
  these tags (auto-tagging is off). An endpoint maps its request to a command
  (`request.ToCommand()`), executes it and sends the response. No business logic in endpoints.
  **All input rules live in validators** (FastEndpoints `Validator<TRequest>`, i.e. FluentValidation):
  required fields, lengths, `EndDate` after `StartDate`. Rules shared by several requests go in one
  `AbstractValidator<IShared>` that each request validator `Include`s (e.g. `EventDetailsValidator`).
  Validation failures return 400 ProblemDetails with an `errors` list.
  Exceptions are mapped to RFC 7807 ProblemDetails in one `IExceptionHandler`:
  `NotFoundException` → 404, `ConflictException` → 409.

### Naming

- `XxxRequest` / `XxxResponse` for everything that crosses HTTP. No separate DTO or ViewModel types:
  the chain is Entity → Command/Query → Response.
- `XxxEndpoint`, `XxxValidator`, `XxxCommand`, `XxxQuery`, `XxxCommandHandler`, `XxxQueryHandler`, `XxxRepository`.
- Avoid names the analysers reserve: no type ending in `EventHandler` (CA1711), no namespace or parameter
  named after a VB keyword such as `Get`, `To`, `Step` (CA1716). Hence `GetById/` and `rangeStart`/`rangeEnd`.

Don't add interfaces with only one implementation unless they sit on a layer boundary (repositories).
Don't add MediatR, AutoMapper or similar libraries: FastEndpoints is the command bus and mapping is by hand.

## Domain conventions

- Names say what a value is: `StartDate`/`EndDate`, not `Start`/`End`. Same name in C#, JSON and the database.
- All times are **UTC** (`DateTime` with `DateTimeKind.Utc`). The API accepts `DateTimeOffset` and
  converts at the edge. Time ranges are half-open: `[StartDate, EndDate)`. Two ranges overlap when
  `a.StartDate < b.EndDate && b.StartDate < a.EndDate`.
- SQLite has no date or uuid types, so tables are `Strict` and use `Text`. Times are fixed-width ISO 8601 text
  (`UtcDateTimeTypeHandler`), so string comparison in SQL
  matches time order. Guids are stored as text (`GuidTypeHandler`).
- IDs are `Guid`, generated in the create handler (`Guid.CreateVersion7()`).
- Length limits live as constants on the entity (e.g. `CalendarEvent.TitleMaxLength`) and are reused
  by request validators.

## Code style

Enforced by `.editorconfig`, `Directory.Build.props` (warnings as errors, .NET analysers in
`Recommended` mode, code style enforced in build) and `Directory.Packages.props`.

- English for all code, comments, docs and commit messages.
- LF line endings everywhere (`.editorconfig` + `.gitattributes`).
- Private fields `_camelCase`; constants and `static readonly` fields `PascalCase`.
- File-scoped namespaces, one public type per file, file name = type name. Only exception: a command/query
  record sits in its handler's file.
- `sealed` classes by default. `record`s for requests, responses, commands and queries.
- Primary constructors for dependency injection.
- Async methods end in `Async`, except FastEndpoints overrides (`HandleAsync` already does).
- Pass `CancellationToken` through every async call chain.
- Use `var` when the type is obvious from the right-hand side.

## Packages

- Versions are pinned centrally in `Directory.Packages.props`. Never put versions in `.csproj` files
  and never use wildcards.
- Target is **.NET 10 (LTS)**. Stay on the 10.x line for Microsoft packages.
- In use: FastEndpoints (+ Swagger, + Messaging.Core), Dapper, Microsoft.Data.Sqlite. FluentValidation
  comes with FastEndpoints.
- Keep dependencies minimal. Ask before adding a new package.

## Testing

- xUnit with plain `Assert`. No assertion libraries or mocking frameworks; write small fakes if needed.
- Test names: `Method_Scenario_ExpectedResult` (e.g. `Validate_EndDateBeforeStartDate_HasEndDateError`).
- Arrange / Act / Assert, separated by blank lines.
- Validators and pure logic (recurrence expansion, overlap, conflicts) get unit tests. Every endpoint gets at least
  happy-path and main error-path integration tests.
- Each integration test gets its own isolated in-memory database (`CalendarApiFactory`).

## Git

- One commit per feature step (plus small follow-ups), so the history shows the progression.
- Never commit `PROGRESS.md`: it holds private notes and is ignored by git.
- Commit messages: imperative mood, e.g. `Add recurrence patterns to events`.
- Never commit `*.db` files, `bin/` or `obj/`.

## Environment notes

- Windows 11, PowerShell, VS Code with C# Dev Kit. No Visual Studio.
- Smart App Control is turned off; it blocks locally built, unsigned DLLs from loading.
