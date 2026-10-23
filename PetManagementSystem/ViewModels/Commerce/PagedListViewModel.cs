using PetManagementSystem.Helpers;

namespace PetManagementSystem.ViewModels.Commerce;

public interface IPagedViewModel
{
    int Page { get; }
    int PageSize { get; }
    int TotalCount { get; }
    int TotalPages { get; }
}

public abstract class PagedListViewModel<TRow> : IPagedViewModel
{
    public List<TRow> Items { get; set; } = [];
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = Paging.DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
