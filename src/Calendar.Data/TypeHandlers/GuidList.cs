namespace Calendar.Data.TypeHandlers;

internal static class GuidList
{
    // Dapper's list expansion (`In @Ids`) skips type handlers, so ids are passed as the text they are stored as.
    public static IEnumerable<string> AsText(IEnumerable<Guid> ids) => ids.Select(id => id.ToString());
}
