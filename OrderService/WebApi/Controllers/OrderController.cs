using Application.Abstractions.Request;
using Application.Orders.Commands;
using Application.Orders.Commands.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models.Orders;

namespace WebApi.Controllers;

public sealed class OrderController : BaseController
{
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] OrderCreateRequestModel model,
        [FromServices] IRequestHandler<OrderCreateCommand, ErrorOr<Guid>> requestHandler,
        CancellationToken cancellationToken)
    {
        var command = new OrderCreateCommand(
            model.CustomerId,
            model.DeliveryAddress,
            model.OrderItems.Adapt<ICollection<OrderItemCreateDto>>());
        
        var result = await requestHandler.HandleAsync(command, cancellationToken);

        return ToActionResult(result, value => Created());
    }
}
