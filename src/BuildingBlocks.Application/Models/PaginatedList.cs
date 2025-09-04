using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Application.Models;

/// <summary>
/// Represents a paginated list of items with metadata about the pagination.
/// Provides information about the current page, total count, and navigation capabilities.
/// </summary>
/// <typeparam name="T">The type of items in the paginated list.</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// Gets the items on the current page.
    /// </summary>
    public IReadOnlyCollection<T> Items { get; }

    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Initializes a new instance of the PaginatedList class.
    /// </summary>
    /// <param name="items">The items on the current page.</param>
    /// <param name="count">The total number of items across all pages.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    public PaginatedList(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Creates a paginated list asynchronously from an IQueryable source.
    /// </summary>
    /// <param name="source">The queryable source to paginate.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation and contains the paginated list.</returns>
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}