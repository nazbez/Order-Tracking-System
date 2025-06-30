using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using KafkaFlow;
using OutboxWorker;
using OutboxWorker.Database;
using OutboxWorker.Kafka;
using OutboxWorker.Models.Events;
using OutboxWorker.Services;
using Serilog;
using Acks = KafkaFlow.Acks;
using SaslMechanism = KafkaFlow.Configuration.SaslMechanism;
using SecurityProtocol = KafkaFlow.Configuration.SecurityProtocol;

var builder = Host.CreateApplicationBuilder(args);

var environment = builder.Environment;

builder.Configuration
    .AddJsonFile("Configurations/appsettings.json")
    .AddJsonFile($"Configurations/appsettings.{environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

if (environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("Configurations/appsettings.Secrets.json", optional: false, reloadOnChange: true);
}

builder.Services.AddSerilog((_, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext());

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection(KafkaOptions.KafkaSectionName));

builder.Services.AddScoped<IEventProducer, EventProducer>();

builder.Services.AddKafka(kafka => 
    kafka.UseMicrosoftLog()
        .AddCluster(cluster => 
            cluster
                .WithBrokers([builder.Configuration["Kafka:BootstrapServers"]])
                .WithSchemaRegistry(configuration => 
                { 
                    configuration.Url = builder.Configuration["SchemaRegistry:URL"] ?? string.Empty;
                    configuration.BasicAuthCredentialsSource = AuthCredentialsSource.UserInfo;
                    configuration.BasicAuthUserInfo = builder.Configuration["SchemaRegistry:BasicAuthUserInfo"] ?? string.Empty;
                })
                .WithSecurityInformation(information => 
                { 
                    information.SaslMechanism = SaslMechanism.Plain; 
                    information.SaslPassword = builder.Configuration["Kafka:SaslPassword"] ?? string.Empty; 
                    information.SaslUsername = builder.Configuration["Kafka:SaslUsername"] ?? string.Empty; 
                    information.SecurityProtocol = SecurityProtocol.SaslSsl; 
                    information.EnableSslCertificateVerification = true; 
                })
                .AddProducer<EventProducer>(producer =>
                    producer
                        .AddMiddlewares(middlewares => 
                            middlewares.AddSchemaRegistryJsonSerializer<OrderCreatedEvent>(new JsonSerializerConfig
                            {
                                SubjectNameStrategy = SubjectNameStrategy.Topic,
                                UseLatestVersion = true,
                                AutoRegisterSchemas = false,
                            }))
                        .WithProducerConfig(new ProducerConfig
                        {
                            EnableMetricsPush = false
                        })
                        .DefaultTopic(builder.Configuration["Kafka:Topic"])
                        .WithCompression(CompressionType.Gzip)
                        .WithAcks(Acks.All))));

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("Sql")!;

    return new DbConnectionFactory(connectionString);
});

builder.Services.AddScoped<IEventProcessor, EventProcessor>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
