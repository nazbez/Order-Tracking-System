using System.Diagnostics.CodeAnalysis;
using Application.CourierOrders.Commands;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using OrderTrackingSystem.Core.Mediator;
using WebApi.Controllers.Base;
using WebApi.Models.CourierOrders;

namespace WebApi.Controllers.Integration;

[ExcludeFromCodeCoverage]
public sealed class IntegrationCourierOrdersController : BaseController
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        IntegrationCourierOrderCreateRequestModel requestModel,
        [FromServices] IRequestHandler<IntegrationCourierOrderCreateCommand, ErrorOr<Guid>> handler,
        CancellationToken cancellationToken)
    {
        var command = new IntegrationCourierOrderCreateCommand(
            requestModel.OrderId,
            requestModel.DeliveryAddress,
            requestModel.CustomerId);
        
        var result = await handler.HandleAsync(command, cancellationToken);

        return ToActionResult(result, _ => Created());
    }
}
