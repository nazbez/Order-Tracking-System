using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace WebApi.GrpcServices.Interceptors;

[ExcludeFromCodeCoverage]
public sealed class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Starting receiving call. Type/Method: {Type} / {Method}", MethodType.Unary, context.Method);
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error thrown by {Method}.", context.Method);
            throw;
        }
    }
}
