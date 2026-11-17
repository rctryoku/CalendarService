using Calendar.Core.Features.Resources;
using Calendar.Core.Features.Resources.Create;
using Dapper;

namespace Calendar.Data.Features.Resources.Create;

internal sealed class CreateResourceRepository(SqliteConnectionFactory connectionFactory) : ICreateResourceRepository
{
    public async Task CreateAsync(Resource resource, CancellationToken cancellationToken)
    {
        var sql = """
            Insert Into Resources
            (
                Id,
                Name
            )
            Values
            (
                @Id,
                @Name
            )
            """;

        await using var connection = await connectionFactory.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, resource, cancellationToken: cancellationToken));
    }
}
