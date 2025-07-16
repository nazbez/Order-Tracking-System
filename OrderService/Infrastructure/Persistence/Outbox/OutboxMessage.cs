using System.Diagnostics.CodeAnalysis;
using OrderTrackingSystem.Core.IntegrationEvents;

namespace Infrastructure.Persistence.Outbox;

[ExcludeFromCodeCoverage]
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; }
    public string Content { get; private set; }
    public DateTimeOffset OccurredOnUtc { get; private set; }
    public DateTimeOffset? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }
    
    public static OutboxMessage From<T>(T integrationEvent) where T : IIntegrationEvent
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(T).Name ?? throw new InvalidOperationException("Integration event type name cannot be null."),
            Content = System.Text.Json.JsonSerializer.Serialize(integrationEvent),
            OccurredOnUtc = DateTimeOffset.UtcNow,
            ProcessedOnUtc = null,
            Error = null
        };
    }
}
