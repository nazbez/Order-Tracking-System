using Grpc.Core;
using Grpc.Net.Client.Configuration;
using OrderTrackingSystem.AspNet.Extensions;
using OrderTrackingSystem.Grpc;
using WebApi.Grpc;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;

builder.Host.AddSerilog();

builder.Configuration.AddConfiguration(environment);
builder.Services.AddWeb(builder.Configuration);

var defaultMethodConfig = new MethodConfig
{
    Names = { MethodName.Default },
    RetryPolicy = new RetryPolicy
    {
        MaxAttempts = 5,
        InitialBackoff = TimeSpan.FromSeconds(1),
        MaxBackoff = TimeSpan.FromSeconds(5),
        BackoffMultiplier = 1.5,
        RetryableStatusCodes = { StatusCode.Unavailable }
    }
};

builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(
        o => 
        { 
            o.Address = new Uri(builder.Configuration.GetValue<string>("Services:OrderService")!);
            o.ChannelOptionsActions.Add(channelOptions =>
            {
                channelOptions.ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } };
            });
        })
    .AddInterceptor<LoggingInterceptor>();

var app = builder.Build();

app.UseWeb();

app.MapGet("api/v1/orders/{orderId:guid}/status",
    async (Guid orderId, OrderService.OrderServiceClient orderServiceClient) =>
    {
        var request = new OrderStatusRequest { OrderId = orderId.ToString() };
        
        try
        {
            var response = await orderServiceClient.GetOrderStatusAsync(request);
            return Results.Ok(response.Status);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return Results.NotFound();
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
        {
            return Results.BadRequest(ex.Status.Detail);
        }
    })
    .WithName("GetOrderStatus")
    .Produces<string>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.Run();
