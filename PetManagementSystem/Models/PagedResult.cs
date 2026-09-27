using System;
using System.Collections.Generic;

namespace PetManagementSystem.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        // Alias tương thích cho code của hsang (Page = PageIndex, TotalItems = TotalCount)
        public int Page
        {
            get => PageIndex;
            set => PageIndex = value;
        }

        public int TotalItems
        {
            get => TotalCount;
            set => TotalCount = value;
        }

        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        // Constructor mặc định
        public PagedResult() { }

        // Constructor 4 tham số (Items, TotalCount/TotalItems, PageIndex/Page, PageSize)
        public PagedResult(List<T> items, int totalCount, int pageIndex, int pageSize)
        {
            Items = items ?? new List<T>();
            TotalCount = totalCount;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
    }
}