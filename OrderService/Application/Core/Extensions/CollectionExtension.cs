namespace Application.Core.Extensions;

public static class CollectionExtension
{
    public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }

        return collection;
    }
}
