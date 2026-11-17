using Calendar.Core.Features.Calendars.Create;
using Calendar.Core.Features.Calendars.Delete;
using Calendar.Core.Features.Calendars.GetById;
using Calendar.Core.Features.Calendars.List;
using Calendar.Core.Features.Calendars.Rename;
using Calendar.Core.Features.Events.Bookings;
using Calendar.Core.Features.Events.Create;
using Calendar.Core.Features.Events.Delete;
using Calendar.Core.Features.Events.GetById;
using Calendar.Core.Features.Events.List;
using Calendar.Core.Features.Events.Update;
using Calendar.Core.Features.Resources.Create;
using Calendar.Core.Features.Resources.Delete;
using Calendar.Core.Features.Resources.GetById;
using Calendar.Core.Features.Resources.List;
using Calendar.Core.Features.Resources.Rename;
using Calendar.Data.Features.Calendars.Create;
using Calendar.Data.Features.Calendars.Delete;
using Calendar.Data.Features.Calendars.GetById;
using Calendar.Data.Features.Calendars.List;
using Calendar.Data.Features.Calendars.Rename;
using Calendar.Data.Features.Events.Bookings;
using Calendar.Data.Features.Events.Create;
using Calendar.Data.Features.Events.Delete;
using Calendar.Data.Features.Events.GetById;
using Calendar.Data.Features.Events.List;
using Calendar.Data.Features.Events.Update;
using Calendar.Data.Features.Resources.Create;
using Calendar.Data.Features.Resources.Delete;
using Calendar.Data.Features.Resources.GetById;
using Calendar.Data.Features.Resources.List;
using Calendar.Data.Features.Resources.Rename;
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
        AddCalendarRepositories(services);
        AddEventRepositories(services);
        AddResourceRepositories(services);
        return services;
    }

    public static Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken) =>
        services.GetRequiredService<DatabaseMigrator>().MigrateAsync(cancellationToken);

    private static void AddCalendarRepositories(IServiceCollection services)
    {
        services.AddSingleton<ICreateCalendarRepository, CreateCalendarRepository>();
        services.AddSingleton<IRenameCalendarRepository, RenameCalendarRepository>();
        services.AddSingleton<IDeleteCalendarRepository, DeleteCalendarRepository>();
        services.AddSingleton<IGetCalendarByIdRepository, GetCalendarByIdRepository>();
        services.AddSingleton<IListCalendarsRepository, ListCalendarsRepository>();
    }

    private static void AddEventRepositories(IServiceCollection services)
    {
        services.AddSingleton<ICreateEventRepository, CreateEventRepository>();
        services.AddSingleton<IUpdateEventRepository, UpdateEventRepository>();
        services.AddSingleton<IDeleteEventRepository, DeleteEventRepository>();
        services.AddSingleton<IGetEventByIdRepository, GetEventByIdRepository>();
        services.AddSingleton<IListEventsRepository, ListEventsRepository>();
        services.AddSingleton<IResourceBookingRepository, ResourceBookingRepository>();
    }

    private static void AddResourceRepositories(IServiceCollection services)
    {
        services.AddSingleton<ICreateResourceRepository, CreateResourceRepository>();
        services.AddSingleton<IRenameResourceRepository, RenameResourceRepository>();
        services.AddSingleton<IDeleteResourceRepository, DeleteResourceRepository>();
        services.AddSingleton<IGetResourceByIdRepository, GetResourceByIdRepository>();
        services.AddSingleton<IListResourcesRepository, ListResourcesRepository>();
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
