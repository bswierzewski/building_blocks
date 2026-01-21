using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class QueryableExtensions
{
    extension<T>(IQueryable<T> source)
    {
        public async Task<PaginatedList<T>> ToPaginatedListAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
        {
            var count = await source.CountAsync(cancellationToken);
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }
    }
}