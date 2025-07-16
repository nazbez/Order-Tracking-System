namespace OrderTrackingSystem.Core.Mediator;

/// <summary>
/// Defines a handler for processing requests of type <typeparamref name="TRequest"/> and returning a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The type of the request object.</typeparam>
/// <typeparam name="TResponse">The type of the response object.</typeparam>
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the given request asynchronously and returns a response.
    /// </summary>
    /// <param name="request">The request object to be processed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the response of type <typeparamref name="TResponse"/>.</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}