Create Table Resources
(
    Id   Text Not Null Primary Key,
    Name Text Not Null
) Strict;

-- Which resources an event books. Not tied to a calendar, so a resource is shared by all calendars.
Create Table EventResources
(
    EventId    Text Not Null References Events (Id) On Delete Cascade,
    ResourceId Text Not Null References Resources (Id) On Delete Cascade,
    Primary Key (EventId, ResourceId)
) Strict;

Create Index IX_EventResources_ResourceId On EventResources (ResourceId);
