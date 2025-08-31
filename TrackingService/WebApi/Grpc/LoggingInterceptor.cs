using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace WebApi.Grpc;

[ExcludeFromCodeCoverage]
public sealed class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Starting call. Type/Method: {Type} / {Method}", context.Method.Type, context.Method.Name);
        return continuation(request, context);
    }
}
