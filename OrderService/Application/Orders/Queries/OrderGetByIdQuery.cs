using Application.Abstractions.Data;
using Application.Orders.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries;

public sealed record OrderGetByIdQuery(Guid Id) : IRequest<ErrorOr<OrderDto>>;

[UsedImplicitly]
public sealed class OrderGetByIdQueryHandler(IApplicationDbContext applicationDbContext)
    : IRequestHandler<OrderGetByIdQuery, ErrorOr<OrderDto>>
{
    public async Task<ErrorOr<OrderDto>> HandleAsync(
        OrderGetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var orderEntity = await applicationDbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .ProjectToType<OrderDto>()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (orderEntity is null)
        {
            return Error.NotFound();
        }

        return orderEntity;
    }
}
