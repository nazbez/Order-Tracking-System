using Application.CourierOrders.Commands;
using Application.CourierOrders.Commands.Validators;

namespace Application.UnitTests.CourierOrders.Commands.Validators;

public class IntegrationCourierOrderCreateCommandValidatorTests
{
    private readonly IntegrationCourierOrderCreateCommandValidator validator = new();
    
    [Fact]
    public void DeliveryAddress_IsInvalidWhenEmpty()
    {
        // Arrange
        var command = new IntegrationCourierOrderCreateCommand(
            Guid.NewGuid(),
            string.Empty,
            Guid.NewGuid());

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DeliveryAddress");
    }
    
    [Fact]
    public void Command_IsValidWhenAllFieldsAreCorrect()
    {
        // Arrange
        var command = new IntegrationCourierOrderCreateCommand(
            Guid.NewGuid(),
            "string",
            Guid.NewGuid());

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
