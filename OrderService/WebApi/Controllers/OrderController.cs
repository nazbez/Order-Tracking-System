using System.Diagnostics.CodeAnalysis;
using Application.Orders.Commands;
using Application.Orders.Models;
using Application.Orders.Queries;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using OrderTrackingSystem.Core.Mediator;
using WebApi.Models.Orders;

namespace WebApi.Controllers;

[ExcludeFromCodeCoverage]
public sealed class OrderController : BaseController
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] IRequestHandler<OrderGetByIdQuery, ErrorOr<OrderDto>> requestHandler,
        CancellationToken cancellationToken)
    {
        var query = new OrderGetByIdQuery(id);
        
        var result = await requestHandler.HandleAsync(query, cancellationToken);

        return ToActionResult(
            result,
            value => Ok(value.Adapt<OrderResponseModel>()));
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] OrderCreateRequestModel model,
        [FromServices] IRequestHandler<OrderCreateCommand, ErrorOr<Guid>> requestHandler,
        CancellationToken cancellationToken)
    {
        var command = new OrderCreateCommand(
            model.CustomerId,
            model.DeliveryAddress,
            model.OrderItems.Adapt<ICollection<OrderItemDto>>());
        
        var result = await requestHandler.HandleAsync(command, cancellationToken);

        return ToActionResult(
            result,
            value => CreatedAtAction(nameof(GetById), new { id = value }, value));
    }
}
