using Calendar.Core.Features.Events.Create;
using Calendar.Core.Features.Events.Delete;
using Calendar.Core.Features.Events.GetById;
using Calendar.Core.Features.Events.List;
using Calendar.Core.Features.Events.Update;
using Calendar.Data.Features.Events.Create;
using Calendar.Data.Features.Events.Delete;
using Calendar.Data.Features.Events.GetById;
using Calendar.Data.Features.Events.List;
using Calendar.Data.Features.Events.Update;
using Calendar.Data.TypeHandlers;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace Calendar.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services, string connectionString)
    {
        RegisterTypeHandlers();

        services.AddSingleton(new SqliteConnectionFactory(connectionString));
        services.AddSingleton<DatabaseMigrator>();
        AddEventRepositories(services);
        return services;
    }

    public static Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken) =>
        services.GetRequiredService<DatabaseMigrator>().MigrateAsync(cancellationToken);

    private static void AddEventRepositories(IServiceCollection services)
    {
        services.AddSingleton<ICreateEventRepository, CreateEventRepository>();
        services.AddSingleton<IUpdateEventRepository, UpdateEventRepository>();
        services.AddSingleton<IDeleteEventRepository, DeleteEventRepository>();
        services.AddSingleton<IGetEventByIdRepository, GetEventByIdRepository>();
        services.AddSingleton<IListEventsRepository, ListEventsRepository>();
    }

    private static void RegisterTypeHandlers()
    {
        // Dapper only consults custom handlers for types it has no built-in mapping for.
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(DateTime));
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new UtcDateTimeTypeHandler());
    }
}
