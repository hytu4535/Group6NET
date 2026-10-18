using PetManagementSystem.Helpers;
using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.ViewModels.Shop;

/// <summary>Tiêu chí tìm kiếm/lọc lấy từ query string; được giữ nguyên khi chuyển trang.</summary>
public class ProductCatalogQuery
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool InStock { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
}

public class CategoryFilterViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProductCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Unit { get; set; }
    public string? CategoryName { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool InStock => StockQuantity > 0;
}

public class ProductListViewModel : PagedListViewModel<ProductCardViewModel>
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool InStock { get; set; }
    public string Sort { get; set; } = ProductSorts.Newest;
    public List<CategoryFilterViewModel> Categories { get; set; } = [];

    public bool HasFilter =>
        !string.IsNullOrWhiteSpace(Search) || CategoryId.HasValue || MinPrice.HasValue || MaxPrice.HasValue || InStock;

    /// <summary>Giá trị route dùng để giữ nguyên bộ lọc khi phân trang.</summary>
    public Dictionary<string, string?> RouteValues => new()
    {
        ["search"] = Search,
        ["categoryId"] = CategoryId?.ToString(),
        ["minPrice"] = MinPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
        ["maxPrice"] = MaxPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
        ["inStock"] = InStock ? "true" : null,
        ["sort"] = Sort == ProductSorts.Newest ? null : Sort
    };
}

public class ProductDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Unit { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal Price { get; set; }

    /// <summary>Số lượng tối đa có thể mua (tồn kho hiện tại).</summary>
    public int StockQuantity { get; set; }
    public bool InStock => StockQuantity > 0;
}
