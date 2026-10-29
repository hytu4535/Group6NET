using PetManagementSystem.Models;
using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Services;

public interface ICustomerService
{
    Task<PagedResult<CustomerListItemViewModel>> SearchAsync(string? search, string? status, string? sort, bool descending, int page, int pageSize);
    Task<CustomerDetailViewModel?> GetDetailsAsync(int id);
    Task<CustomerFormViewModel?> GetFormAsync(int id);
    Task<CustomerFormViewModel> PrepareCreateFormAsync();
    Task<CustomerUserLookupViewModel?> GetUserLookupAsync(int userId);
    Task<(bool Success, string ErrorMessage)> CreateAsync(CustomerFormViewModel model);
    Task<(bool Success, string ErrorMessage)> UpdateAsync(CustomerFormViewModel model);
    Task<(bool Success, string ErrorMessage)> ToggleStatusAsync(int id);
    Task<(bool Success, string ErrorMessage)> DeleteAsync(int id);
}
