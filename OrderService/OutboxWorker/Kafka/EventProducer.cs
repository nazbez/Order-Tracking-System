using System.Diagnostics.CodeAnalysis;
using KafkaFlow;
using KafkaFlow.Producers;
using OutboxWorker.Models.Events;

namespace OutboxWorker.Kafka;

public interface IEventProducer
{
    Task BatchProduceAsync<TKey, TEvent>(IEnumerable<Message<TKey,TEvent>> messages, CancellationToken cancellationToken = default)
        where TEvent : Models.Events.IEvent<TKey> where TKey : struct;
}

[ExcludeFromCodeCoverage]
public sealed class EventProducer(IMessageProducer<EventProducer> producer) : IEventProducer
{
    public Task BatchProduceAsync<TKey, TEvent>(IEnumerable<Message<TKey, TEvent>> messages, CancellationToken cancellationToken = default) where TKey : struct where TEvent : Models.Events.IEvent<TKey>
    {
        var items = messages.Select(m => 
                new BatchProduceItem(m.Topic, m.Key, m.Event, new MessageHeaders()))
            .ToList();

        return producer.BatchProduceAsync(items);
    }
}
