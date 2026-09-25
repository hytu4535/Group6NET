using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public interface IProductService
{
    Task<ProductListViewModel> GetListAsync(string? search, int? categoryId, string? status, int page);
    Task<ProductUpsertViewModel> BuildCreateModelAsync();
    Task<ProductUpsertViewModel?> GetForEditAsync(int id);

    /// <summary>Nạp lại danh sách danh mục khi hiển thị lại form sau lỗi validate.</summary>
    Task PopulateOptionsAsync(ProductUpsertViewModel model);

    Task<ServiceResult> CreateAsync(ProductUpsertViewModel model);
    Task<ServiceResult> UpdateAsync(ProductUpsertViewModel model);
    Task<ServiceResult> ToggleStatusAsync(int id);
    Task<ServiceResult> DeleteAsync(int id);
}
