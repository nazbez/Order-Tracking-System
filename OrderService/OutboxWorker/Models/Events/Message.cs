namespace OutboxWorker.Models.Events;

public sealed class Message<TKey, TEvent>(TEvent @event, string topic)
    where TKey : struct
    where TEvent : IEvent<TKey>
{
    public string Key { get; private set; } = @event.Id.ToString()!;
    public string Topic { get; private set; } = topic;
    public TEvent Event { get; private set; } = @event;
}
