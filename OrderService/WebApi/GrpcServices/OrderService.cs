using System.Diagnostics.CodeAnalysis;
using Application.Abstractions.Data;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using OrderTrackingSystem.Grpc;
using OrderServiceBase = OrderTrackingSystem.Grpc.OrderService.OrderServiceBase;

namespace WebApi.GrpcServices;

[ExcludeFromCodeCoverage]
public sealed class OrderService(IApplicationDbContext applicationDbContext) : OrderServiceBase
{
    public override async Task<OrderStatusResponse> GetOrderStatus(OrderStatusRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid OrderId format."));
        }
        
        var orderEntity = await applicationDbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, context.CancellationToken);

        if (orderEntity is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Order not found."));
        }
        
        var response = new OrderStatusResponse
        {
            OrderId = orderEntity.Id.ToString(),
            Status = orderEntity.Status.ToString(),
            CreatedAt = Timestamp.FromDateTimeOffset(orderEntity.CreatedAt),
            UpdatedAt = Timestamp.FromDateTimeOffset(orderEntity.UpdatedAt)
        };

        return response;
    }
}
