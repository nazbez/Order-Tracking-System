namespace Application.CourierOrders.Commands.Validators;

[UsedImplicitly]
public sealed class IntegrationCourierOrderCreateCommandValidator : AbstractValidator<IntegrationCourierOrderCreateCommand>
{
    public IntegrationCourierOrderCreateCommandValidator()
    {
        RuleFor(cmd => cmd.DeliveryAddress)
            .NotEmpty();
    }
}
