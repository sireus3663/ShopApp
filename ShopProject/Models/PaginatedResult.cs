using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopProject.Models
{
    public class PaginatedResult<T>
    {
        public List<T> Items { get; }
        public int TotalCount { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public bool IsEmpty => Items == null || Items.Count == 0;

        public PaginatedResult(List<T> items, int totalCount, int page, int pageSize)
        {
            Items = items ?? new List<T>();
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }

        public static PaginatedResult<T> Empty()
        {
            return new PaginatedResult<T>(new List<T>(), 0, 1, 10);
        }

        public PaginatedResult<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            var newItems = Items.Select(mapper).ToList();
            return new PaginatedResult<TNew>(newItems, TotalCount, Page, PageSize);
        }

        public override string ToString()
        {
            return $"Страница {Page} из {TotalPages}, элементов: {Items.Count} из {TotalCount}";
        }
    }
}