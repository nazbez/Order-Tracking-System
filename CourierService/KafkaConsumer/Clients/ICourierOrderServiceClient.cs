using KafkaConsumer.Models.Requests;
using Refit;

namespace KafkaConsumer.Clients;

public interface ICourierOrderServiceClient
{
    [Post("/api/v1/integration-courier-orders")]
    Task CreateAsync([Body] CourierOrderCreateModel request, CancellationToken cancellationToken = default);
}
