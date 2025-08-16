using Application.Abstractions.Data;
using Application.Orders.IntegrationEvents;
using Application.Orders.Models;
using Domain.Orders;

namespace Application.Orders.Commands;

public sealed record OrderCreateCommand(
    Guid CustomerId,
    string DeliveryAddress,
    ICollection<OrderItemDto> OrderItems) : IRequest<ErrorOr<Guid>>;

[UsedImplicitly]
public sealed class OrderCreateCommandHandler(
    IApplicationDbContext applicationDbContext,
    IIntegrationEventPublisher integrationEventPublisher)
    : IRequestHandler<OrderCreateCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> HandleAsync(
        OrderCreateCommand request,
        CancellationToken cancellationToken)
    {
        var order = Order.Create(request.DeliveryAddress, request.CustomerId);
        
        foreach (var orderItem in request.OrderItems)
        {
            order.AddOrderItem(orderItem.ProductName, orderItem.Price, orderItem.Quantity);
        }

        await applicationDbContext.Orders.AddAsync(order, cancellationToken);
        
        await integrationEventPublisher.Publish(new OrderCreatedIntegrationEvent(
                order.Id, order.CustomerId, order.CreatedAt, order.DeliveryAddress),
            cancellationToken);
        
        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
