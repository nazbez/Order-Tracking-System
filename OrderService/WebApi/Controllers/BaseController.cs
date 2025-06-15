using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v{v:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public abstract class BaseController : ControllerBase
{
    protected IActionResult ToActionResult<T>(ErrorOr<T> errorOrResult, Func<T, IActionResult> onSuccess)
    {
        return errorOrResult.MatchFirst(onSuccess, error =>
        {
            return error.Type switch
            {
                ErrorType.Validation => BadRequest(error),
                ErrorType.NotFound => NotFound(error),
                ErrorType.Conflict => Conflict(error),
                _ => StatusCode(StatusCodes.Status500InternalServerError, error)
            };
        });
    }
}
