using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Data;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderTrackingSystem.Core.IntegrationEvents;

namespace Infrastructure;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Sql")!;
        
        serviceCollection.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        serviceCollection.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        
        serviceCollection.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();

        serviceCollection.AddHealthChecks().AddNpgSql(connectionString);
    }
}
