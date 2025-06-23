using OutboxWorker;
using OutboxWorker.Database;
using OutboxWorker.Services;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

var environment = builder.Environment;

builder.Configuration
    .AddJsonFile("Configurations/appsettings.json")
    .AddJsonFile($"Configurations/appsettings.{environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext());

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("Sql")!;

    return new DbConnectionFactory(connectionString);
});

builder.Services.AddScoped<IEventProcessor, EventProcessor>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
