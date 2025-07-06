using System.Text;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using KafkaConsumer.Clients;
using KafkaConsumer.Handlers;
using KafkaConsumer.Models;
using KafkaConsumer.Models.Events;
using KafkaFlow;
using Refit;
using Serilog;
using AutoOffsetReset = KafkaFlow.AutoOffsetReset;
using SaslMechanism = KafkaFlow.Configuration.SaslMechanism;
using SecurityProtocol = KafkaFlow.Configuration.SecurityProtocol;

var builder = Host.CreateApplicationBuilder(args);

var environment = builder.Environment;

builder.Configuration
    .AddJsonFile("Configurations/appsettings.json")
    .AddJsonFile($"Configurations/appsettings.{environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

if (environment.IsDevelopment() && File.Exists("Configurations/appsettings.Secrets.json"))
{
    builder.Configuration.AddJsonFile("Configurations/appsettings.Secrets.json", optional: false, reloadOnChange: true);
}

builder.Services.AddSerilog((_, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext());

builder.Services.AddRefitClient<ICourierOrderServiceClient>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!);
    });

builder.Services.AddKafka(kafka =>
    kafka.UseMicrosoftLog()
        .AddCluster(cluster =>
            cluster
                .WithBrokers([builder.Configuration["Kafka:BootstrapServers"]])
                .WithSchemaRegistry(configuration =>
                {
                    configuration.Url = builder.Configuration["SchemaRegistry:URL"] ?? string.Empty;
                    configuration.BasicAuthCredentialsSource = AuthCredentialsSource.UserInfo;
                    configuration.BasicAuthUserInfo =
                        builder.Configuration["SchemaRegistry:BasicAuthUserInfo"] ?? string.Empty;
                })
                .WithSecurityInformation(information =>
                {
                    information.SaslMechanism = SaslMechanism.Plain;
                    information.SaslPassword = builder.Configuration["Kafka:SaslPassword"] ?? string.Empty;
                    information.SaslUsername = builder.Configuration["Kafka:SaslUsername"] ?? string.Empty;
                    information.SecurityProtocol = SecurityProtocol.SaslSsl;
                    information.EnableSslCertificateVerification = true;
                })
                .AddConsumer(consumer => consumer
                    .Topic(builder.Configuration["Kafka:Topic"] ?? string.Empty)
                    .WithGroupId("courier-service-consumer")
                    .WithBufferSize(1)
                    .WithWorkersCount(1)
                    .WithConsumerConfig(new ConsumerConfig
                    {
                        EnableMetricsPush = false
                    })
                    .WithAutoOffsetReset(AutoOffsetReset.Latest)
                    .AddMiddlewares(middleware => 
                        middleware
                            .AddSchemaRegistryJsonSerializer<OrderCreatedEvent>()
                            .AddTypedHandlers(handlers => 
                                handlers
                                    .AddHandler<OrderCreatedEventHandler>()
                                    .WithHandlerLifetime(InstanceLifetime.Scoped))))));

var host = builder.Build();

var kafkaBus = host.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

host.Run();
