namespace OrderTrackingSystem.Core.Mediator;

/// <summary>
/// Represents a request with a specified response type.
/// </summary>
/// <typeparam name="TResponse">The type of the response associated with the request.</typeparam>
public interface IRequest<TResponse>;