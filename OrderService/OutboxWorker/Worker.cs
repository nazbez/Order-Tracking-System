using System.Diagnostics.CodeAnalysis;
using OutboxWorker.Services;

namespace OutboxWorker;

[ExcludeFromCodeCoverage]
public class Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = serviceScopeFactory.CreateScope();
            
            var eventProcessor = scope.ServiceProvider.GetRequiredService<IEventProcessor>();

            await eventProcessor.ProcessAsync(stoppingToken);
        }

        logger.LogInformation("Timed Hosted Service is stopping.");
    }
}
