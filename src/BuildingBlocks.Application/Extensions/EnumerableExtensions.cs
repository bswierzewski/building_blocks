using BuildingBlocks.Application.Models;

namespace BuildingBlocks.Application.Extensions;

/// <summary>
/// Extension methods for IEnumerable to provide additional functionality for collections.
/// These extensions help with common operations on enumerable collections.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Converts an enumerable to a paginated list.
    /// This method loads all items into memory before applying pagination.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated list containing the specified page of items.</returns>
    public static PaginatedList<T> ToPaginatedList<T>(
        this IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        var list = source.ToList();
        var totalCount = list.Count;

        var skip = Math.Max(0, (pageNumber - 1) * pageSize);
        var items = list.Skip(skip).Take(pageSize);

        return new PaginatedList<T>(items.ToList(), totalCount, pageNumber, pageSize);
    }


    /// <summary>
    /// Applies conditional filtering to the enumerable source.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <param name="condition">The condition to check before applying the filter.</param>
    /// <param name="predicate">The filter predicate to apply.</param>
    /// <returns>The enumerable with the conditional filter applied.</returns>
    public static IEnumerable<T> WhereIf<T>(
        this IEnumerable<T> source,
        bool condition,
        Func<T, bool> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }

    /// <summary>
    /// Determines whether the enumerable contains any elements, handling null sources safely.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <returns>True if the source contains any elements; otherwise, false.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    /// <summary>
    /// Determines whether the enumerable contains elements, handling null sources safely.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <returns>True if the source is not null and contains elements; otherwise, false.</returns>
    public static bool HasElements<T>(this IEnumerable<T>? source)
    {
        return source != null && source.Any();
    }

    /// <summary>
    /// Executes an action for each element in the enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <param name="action">The action to execute for each element.</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes an action for each element in the enumerable with the element's index.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <param name="action">The action to execute for each element with its index.</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        var index = 0;
        foreach (var item in source)
        {
            action(item, index);
            index++;
        }
    }

    /// <summary>
    /// Splits the enumerable into chunks of the specified size.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <param name="chunkSize">The size of each chunk.</param>
    /// <returns>An enumerable of chunks, where each chunk is a list of elements.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when chunk size is less than 1.</exception>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
    {
        if (chunkSize < 1)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be greater than 0.");

        var list = source.ToList();
        for (var i = 0; i < list.Count; i += chunkSize)
        {
            yield return list.Skip(i).Take(chunkSize);
        }
    }

    /// <summary>
    /// Returns distinct elements from the enumerable using a key selector.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <typeparam name="TKey">The type of the key used for comparison.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <param name="keySelector">The function to extract the key for comparison.</param>
    /// <returns>An enumerable containing distinct elements based on the key selector.</returns>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();
        foreach (var element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    /// <summary>
    /// Converts the enumerable to a readonly collection.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <returns>A readonly collection containing all elements from the source.</returns>
    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
    {
        return source.ToList().AsReadOnly();
    }

    /// <summary>
    /// Converts the enumerable to a readonly list.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable source.</param>
    /// <returns>A readonly list containing all elements from the source.</returns>
    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> source)
    {
        return source.ToList().AsReadOnly();
    }
}