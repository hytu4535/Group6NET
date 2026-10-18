using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Services;

public interface IStorefrontService
{
    Task<ProductListViewModel> GetProductsAsync(ProductCatalogQuery query);

    /// <summary>Chi tiết sản phẩm đang bán; null nếu không tồn tại hoặc đã ngừng bán.</summary>
    Task<ProductDetailViewModel?> GetProductAsync(int id);
}
