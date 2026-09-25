using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public interface ICategoryService
{
    Task<CategoryListViewModel> GetListAsync(string? search, int page);
    Task<CategoryUpsertViewModel?> GetForEditAsync(int id);
    Task<ServiceResult> CreateAsync(CategoryUpsertViewModel model);
    Task<ServiceResult> UpdateAsync(CategoryUpsertViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
}
