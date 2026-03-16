using IntelligentGateway.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.AI;
using StackExchange.Redis;

namespace IntelligentGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIntelligentGatewayInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AdapterOptions>(configuration.GetSection("Adapters"));

        services.AddSingleton<IToolRegistry, InMemoryToolRegistry>();
        services.AddSingleton<IToolPolicyService, SimpleToolPolicyService>();
        services.AddSingleton<IToolArgumentValidator, JsonSchemaToolArgumentValidator>();

        services.AddHttpClient<OrderServiceAdapter>();
        services.AddHttpClient<TrackingServiceAdapter>();
        services.AddSingleton<CourierServiceAdapter>();
        services.AddSingleton<IServiceAdapter>(sp => sp.GetRequiredService<OrderServiceAdapter>());
        services.AddSingleton<IServiceAdapter>(sp => sp.GetRequiredService<TrackingServiceAdapter>());
        services.AddSingleton<IServiceAdapter>(sp => sp.GetRequiredService<CourierServiceAdapter>());
        services.AddSingleton<IToolExecutor, ToolExecutor>();

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            try
            {
                var multiplexer = ConnectionMultiplexer.Connect(redisConnection);
                services.AddSingleton<IConnectionMultiplexer>(multiplexer);
                services.AddSingleton<ISessionStore, RedisSessionStore>();
            }
            catch
            {
                services.AddSingleton<ISessionStore, InMemorySessionStore>();
            }
        }
        else
        {
            services.AddSingleton<ISessionStore, InMemorySessionStore>();
        }

        services.TryAddSingleton<IChatClient, NoOpChatClient>();
        services.AddSingleton<IChatOrchestrator, RuleBasedChatOrchestrator>();

        return services;
    }
}
