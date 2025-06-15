namespace Application.Core.Decorators;

public sealed class ValidationDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> decoratedHandler,
    IValidator<TRequest>? validator = null)
    : IRequestHandler<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (validator is null)
        {
            return await decoratedHandler.HandleAsync(request, cancellationToken);
        }
        
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return await decoratedHandler.HandleAsync(request, cancellationToken);
        }

        var errors = validationResult.Errors
            .ConvertAll(error => Error.Validation(
                code: error.PropertyName,
                description: error.ErrorMessage));

        return (dynamic)errors;
    }
}
