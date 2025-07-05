using Application.Abstractions.Data;
using Domain.CourierOrders;

namespace Application.CourierOrders.Commands;

public sealed record IntegrationCourierOrderCreateCommand(Guid OrderId, string DeliveryAddress, Guid CustomerId)
    : IRequest<ErrorOr<Guid>>;

[UsedImplicitly]
public sealed class IntegrationCourierOrderCreateCommandHandler(IApplicationDbContext applicationDbContext)
    : IRequestHandler<IntegrationCourierOrderCreateCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> HandleAsync(IntegrationCourierOrderCreateCommand request, CancellationToken cancellationToken)
    {
        var courierOrder = CourierOrder.Create(
            request.OrderId,
            request.DeliveryAddress,
            request.CustomerId);

        await applicationDbContext.CourierOrders.AddAsync(courierOrder, cancellationToken);

        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return courierOrder.Id;
    }
}
