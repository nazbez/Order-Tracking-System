namespace OrderTrackingSystem.Core.Extensions;

/// <summary>
/// Provides extension methods for working with collections.
/// </summary>
public static class CollectionExtension
{
    /// <summary>
    /// Adds a range of items to the specified collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to which the items will be added.</param>
    /// <param name="items">The items to add to the collection.</param>
    /// <returns>The collection with the added items.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="collection"/> or <paramref name="items"/> is <c>null</c>.
    /// </exception>
    public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(items);
        
        foreach (var item in items)
        {
            collection.Add(item);
        }

        return collection;
    }
}