using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OrderTrackingSystem.AspNet.ExceptionHandlers;

/// <summary>
/// Handles global exceptions and generates appropriate problem details responses.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle the given exception and write a problem details response.
    /// </summary>
    /// <param name="httpContext">The HTTP context of the current request.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing a boolean value indicating whether the exception was handled.
    /// </returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Create a ProblemDetails object with information about the exception.
        var problemDetails = new ProblemDetails
        {
            Status = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            },
            Title = "An error occurred",
            Type = exception.GetType().Name,
            Detail = exception.Message
        };

        // Attempt to write the problem details response using the provided service.
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            Exception = exception,
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }
}
