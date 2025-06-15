using Application.Abstractions.Data;
using Application.Core.Extensions;
using Application.Orders.Models;

namespace Application.Orders.Commands;

public sealed record OrderCreateCommand(
    Guid CustomerId,
    string DeliveryAddress,
    ICollection<OrderItemDto> OrderItems) : IRequest<ErrorOr<Guid>>;

[UsedImplicitly]
public sealed class OrderCreateCommandHandler(IApplicationDbContext applicationDbContext)
    : IRequestHandler<OrderCreateCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> HandleAsync(
        OrderCreateCommand request,
        CancellationToken cancellationToken)
    {
        var orderEntity = Order.Create(request.DeliveryAddress, request.CustomerId);

        var orderItemEntities = request.OrderItems
            .Select(oi => OrderItem.Create(oi.ProductName, oi.Price, oi.Quantity));

        orderEntity.OrderItems.AddRange(orderItemEntities);

        await applicationDbContext.Orders.AddAsync(orderEntity, cancellationToken);

        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return orderEntity.Id;
    }
}
