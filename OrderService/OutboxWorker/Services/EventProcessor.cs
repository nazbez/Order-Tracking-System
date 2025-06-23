using Dapper;
using OutboxWorker.Database;
using OutboxWorker.Models;

namespace OutboxWorker.Services;

public interface IEventProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken);
}

public sealed class EventProcessor(
    ILogger<EventProcessor> logger,
    IDbConnectionFactory dbConnectionFactory) : IEventProcessor
{
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

            // Implement send logic

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

            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing outbox messages.");
            transaction.Rollback();
        }
        finally
        {
            logger.LogInformation("Outbox messages are processed.");
        }
    }
}
