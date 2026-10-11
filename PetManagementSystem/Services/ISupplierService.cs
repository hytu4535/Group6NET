using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public interface ISupplierService
{
    Task<SupplierListViewModel> GetListAsync(string? search, int page);
    Task<SupplierUpsertViewModel?> GetForEditAsync(int id);
    Task<ServiceResult> CreateAsync(SupplierUpsertViewModel model);
    Task<ServiceResult> UpdateAsync(SupplierUpsertViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
}
