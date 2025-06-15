using Application.Core.Extensions;

namespace Application.Tests.Core.Extensions;

public sealed class CollectionExtensionTests
{
    [Fact]
    public void AddRange_AddsAllItemsToCollection()
    {
        // Arrange
        ICollection<int> collection = [1, 2, 3];
        var itemsToAdd = new List<int> { 4, 5, 6 };

        // Act
        collection.AddRange(itemsToAdd);

        // Assert
        Assert.Equal(6, collection.Count);
        Assert.Contains(4, collection);
        Assert.Contains(5, collection);
        Assert.Contains(6, collection);
    }

    [Fact]
    public void AddRange_AddsNoItemsWhenSourceIsEmpty()
    {
        // Arrange
        ICollection<int> collection = [1, 2, 3];

        // Act
        collection.AddRange(new List<int>());

        // Assert
        Assert.Equal(3, collection.Count);
        Assert.DoesNotContain(0, collection);
    }

    [Fact]
    public void AddRange_ThrowsArgumentNullExceptionWhenCollectionIsNull()
    {
        // Arrange
        ICollection<int> collection = null!;
        var itemsToAdd = new List<int> { 1, 2, 3 };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection.AddRange(itemsToAdd));
    }

    [Fact]
    public void AddRange_ThrowsArgumentNullExceptionWhenItemsIsNull()
    {
        // Arrange
        List<int> itemsToAdd = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Array.Empty<int>().AddRange(itemsToAdd));
    }
}
