-- Every event now belongs to a calendar. SQLite can't add a required foreign key column to an existing
-- table, so Events is rebuilt. Events created before this change are moved to a "Default" calendar.
Insert Into Calendars
(
    Id,
    Name,
    Owner
)
Select
    '00000000-0000-0000-0000-000000000001',
    'Default',
    'unknown'
Where
    Exists (Select 1 From Events);

Create Table EventsWithCalendar
(
    Id          Text Not Null Primary Key,
    CalendarId  Text Not Null References Calendars (Id) On Delete Cascade,
    Title       Text Not Null,
    Description Text Null,
    StartDate   Text Not Null,
    EndDate     Text Not Null
) Strict;

Insert Into EventsWithCalendar
(
    Id,
    CalendarId,
    Title,
    Description,
    StartDate,
    EndDate
)
Select
    Id,
    '00000000-0000-0000-0000-000000000001',
    Title,
    Description,
    StartDate,
    EndDate
From
    Events;

Drop Table Events;

Alter Table EventsWithCalendar Rename To Events;

Create Index IX_Events_CalendarId_StartDate On Events (CalendarId, StartDate);
