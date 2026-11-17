using Calendar.Core.Features.Resources;
using Calendar.Core.Features.Resources.Rename;
using Dapper;

namespace Calendar.Data.Features.Resources.Rename;

internal sealed class RenameResourceRepository(SqliteConnectionFactory connectionFactory) : IRenameResourceRepository
{
    public async Task<Resource?> RenameAsync(Guid id, string name, CancellationToken cancellationToken)
    {
        var sql = """
            Update
                Resources
            Set
                Name = @Name
            Where
                Id = @Id
            Returning
                Id,
                Name
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Resource>(
            new CommandDefinition(sql, new { Id = id, Name = name }, cancellationToken: cancellationToken));
    }
}
