Create Table Calendars
(
    Id    Text Not Null Primary Key,
    Name  Text Not Null,
    Owner Text Not Null
) Strict;

Create Index IX_Calendars_Owner On Calendars (Owner);
