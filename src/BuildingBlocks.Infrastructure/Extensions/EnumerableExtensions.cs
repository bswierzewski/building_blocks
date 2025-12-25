using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Infrastructure.Models;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public PaginatedList<T> ToPaginatedList(
        int pageNumber,
        int pageSize)
        {
            var list = source.ToList();
            var totalCount = list.Count;

            var skip = Math.Max(0, (pageNumber - 1) * pageSize);
            var items = list.Skip(skip).Take(pageSize);

            return new PaginatedList<T>([.. items], totalCount, pageNumber, pageSize);
        }
    }
}