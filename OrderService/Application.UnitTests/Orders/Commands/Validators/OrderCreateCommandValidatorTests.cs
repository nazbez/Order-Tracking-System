using Application.Orders.Commands;
using Application.Orders.Commands.Validators;
using Application.Orders.Models;

namespace Application.UnitTests.Orders.Commands.Validators;

public sealed class OrderCreateCommandValidatorTests
{
    private readonly OrderCreateCommandValidator validator = new();
    
    [Fact]
    public void DeliveryAddress_IsInvalidWhenEmpty()
    {
        // Arrange
        var command = new OrderCreateCommand(
            Guid.NewGuid(),
            string.Empty,
            new List<OrderItemDto>());

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DeliveryAddress");
    }
    
    [Fact]
    public void OrderItems_AreInvalidWhenPriceIsZeroOrNegative()
    {
        // Arrange
        var command = new OrderCreateCommand(
            Guid.NewGuid(),
            "123 Main St",
            new List<OrderItemDto> 
            {
                new() { Price = 0, Quantity = 1, ProductName = "Product A" },
                new() { Price = -5, Quantity = 1, ProductName = "Product B" }
            }
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OrderItems[0].Price");
        Assert.Contains(result.Errors, e => e.PropertyName == "OrderItems[1].Price");
    }
    
    [Fact]
    public void OrderItems_AreInvalidWhenQuantityIsZeroOrNegative()
    {
        // Arrange
        var command = new OrderCreateCommand(
            Guid.NewGuid(),
            "123 Main St",
            new List<OrderItemDto>
            {
                new() { Price = 10, Quantity = 0, ProductName = "Product A" },
                new() { Price = 10, Quantity = -1, ProductName = "Product B" }
            }
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OrderItems[0].Quantity");
        Assert.Contains(result.Errors, e => e.PropertyName == "OrderItems[1].Quantity");
    }
    
    [Fact]
    public void OrderItems_AreInvalidWhenProductNameIsEmpty()
    {
        // Arrange
        var command = new OrderCreateCommand(
            Guid.NewGuid(), 
            "123 Main St",
            new List<OrderItemDto>
            {
                new() { Price = 10, Quantity = 1, ProductName = string.Empty }
            }
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "OrderItems[0].ProductName");
    }
    
    [Fact]
    public void Command_IsValidWhenAllFieldsAreCorrect()
    {
        // Arrange
        var command = new OrderCreateCommand(
            Guid.NewGuid(), 
            "123 Main St",
            new List<OrderItemDto>
            {
                new() { Price = 10, Quantity = 1, ProductName = "Product A" }
            }
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
