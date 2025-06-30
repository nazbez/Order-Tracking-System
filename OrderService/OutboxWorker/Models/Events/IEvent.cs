namespace OutboxWorker.Models.Events;

public interface IEvent<TId> where TId : struct
{
    public TId Id { get; set; }
}
