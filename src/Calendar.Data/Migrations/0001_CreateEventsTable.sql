-- SQLite has no date or uuid types: ids and UTC times are stored as text (ISO 8601 for times).
-- Strict makes SQLite enforce the declared column types.
Create Table Events
(
    Id          Text Not Null Primary Key,
    Title       Text Not Null,
    Description Text Null,
    StartDate   Text Not Null,
    EndDate     Text Not Null
) Strict;

Create Index IX_Events_StartDate On Events (StartDate);
