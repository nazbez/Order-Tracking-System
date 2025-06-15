namespace Application.Orders.Commands.Validators;

[UsedImplicitly]
public sealed class OrderCreateCommandValidator : AbstractValidator<OrderCreateCommand>
{
    public OrderCreateCommandValidator()
    {
        RuleFor(cmd => cmd.DeliveryAddress)
            .NotEmpty();

        RuleForEach(cmd => cmd.OrderItems)
            .ChildRules(validator =>
            {
                validator.RuleFor(oi => oi.Price)
                    .GreaterThan(0);

                validator.RuleFor(oi => oi.Quantity)
                    .GreaterThan(0);

                validator.RuleFor(oi => oi.ProductName)
                    .NotEmpty();
            });
    } 
}
