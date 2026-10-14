using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public record ProductFilter(
    string? Search,
    int? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool InStockOnly,
    string Sort);

/// <summary>Truy vấn danh mục sản phẩm dành cho khách hàng (chỉ sản phẩm đang bán).</summary>
public interface IStorefrontRepository
{
    Task<PagedResult<Product>> SearchAsync(ProductFilter filter, int page, int pageSize);
    Task<Product?> GetSellableByIdAsync(int id);
    Task<List<Category>> GetCategoriesWithSellableProductsAsync();
}
