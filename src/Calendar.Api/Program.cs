using Calendar.Api.ErrorHandling;
using Calendar.Application.Features.Events.Create;
using Calendar.Data;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Calendar")
    ?? throw new InvalidOperationException("Connection string 'Calendar' is missing.");

builder.Services
    .AddData(connectionString)
    .AddFastEndpoints(options => options.Assemblies = [typeof(CreateEventCommandHandler).Assembly])
    .SwaggerDocument(options => options.DocumentSettings = settings => settings.Title = "Calendar API")
    .AddProblemDetails()
    .AddExceptionHandler<ProblemDetailsExceptionHandler>();

var app = builder.Build();

await app.Services.MigrateDatabaseAsync(app.Lifetime.ApplicationStopping);

app.UseExceptionHandler();
app.UseFastEndpoints(config => config.Errors.UseProblemDetails());

if (app.Environment.IsDevelopment())
    app.UseSwaggerGen();

await app.RunAsync();

public partial class Program;
