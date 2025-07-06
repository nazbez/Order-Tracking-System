using System.Diagnostics.CodeAnalysis;
using KafkaConsumer.Clients;
using KafkaConsumer.Models.Events;
using KafkaConsumer.Models.Requests;
using KafkaFlow;

namespace KafkaConsumer.Handlers;

[ExcludeFromCodeCoverage]
public sealed class OrderCreatedEventHandler(ICourierOrderServiceClient courierOrderServiceClient, ILogger<OrderCreatedEventHandler> logger)
    : IMessageHandler<OrderCreatedEvent>
{
    public async Task Handle(IMessageContext context, OrderCreatedEvent message)
    {
        try
        {
            var request = new CourierOrderCreateModel(message.Id, message.CustomerId, message.DeliveryAddress);
            await courierOrderServiceClient.CreateAsync(request);
            logger.LogInformation("Order created with ID {OrderId} and Customer ID {CustomerId} was handled.",
                message.Id, message.CustomerId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling OrderCreatedEvent with ID {OrderId} and Customer ID {CustomerId}: {ErrorMessage}",
                message.Id, message.CustomerId, e.Message);
            throw;
        }
    }
}
