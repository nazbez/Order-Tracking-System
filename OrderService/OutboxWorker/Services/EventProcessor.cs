using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Options;
using OutboxWorker.Database;
using OutboxWorker.Kafka;
using OutboxWorker.Models;
using OutboxWorker.Models.Events;

namespace OutboxWorker.Services;

public interface IEventProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage]
public sealed class EventProcessor(
    ILogger<EventProcessor> logger,
    IEventProducer eventProducer,
    IOptions<KafkaOptions> kafkaOptions,
    IDbConnectionFactory dbConnectionFactory) : IEventProcessor
{
    private readonly KafkaOptions options = kafkaOptions.Value; 
    
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        using var transaction = connection.BeginTransaction();

        try
        {
            const string selectOutboxMessagesQuery = """
                                                     SELECT * FROM public."OutboxMessages" 
                                                     WHERE "ProcessedOnUtc" IS NULL
                                                     """;

            var outboxMessages = (await connection.QueryAsync<OutboxMessageEntity>(
                    selectOutboxMessagesQuery,
                    transaction))
                .AsList();

            var messages = outboxMessages.Select(x =>
            {
                var @event = JsonSerializer.Deserialize<OrderCreatedEvent>(x.Content)
                             ?? throw new InvalidOperationException("Failed to deserialize event content.");
            
                return new Message<Guid, OrderCreatedEvent>(@event, options.Topic);
            }).ToList();

            await eventProducer.BatchProduceAsync(messages);

            const string updateOutboxMessageQuery = """
                                                    UPDATE public."OutboxMessages" 
                                                    SET "ProcessedOnUtc" = @ProcessedOnUtc 
                                                    WHERE "Id" = ANY(@Ids)
                                                    """;

            await connection.ExecuteAsync(
                updateOutboxMessageQuery,
                new
                {
                    ProcessedOnUtc = DateTimeOffset.Now,
                    Ids = outboxMessages.Select(om => om.Id).AsList()
                },
                transaction);
            
            messages.ForEach(m => 
                logger.LogInformation("Processed outbox message with Id: {Key}, Event: {Event}", 
                    m.Key, 
                    m.Event.GetType().Name));
            
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing outbox messages.");
            transaction.Rollback();
        }
    }
}
