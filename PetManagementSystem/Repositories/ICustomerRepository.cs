using PetManagementSystem.Models;
using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Repositories;

public interface ICustomerRepository
{
    Task<PagedResult<CustomerListItemViewModel>> SearchAsync(
        string? search,
        string? status,
        string? sort,
        bool descending,
        int page,
        int pageSize);
    Task<User?> GetByIdAsync(int id);
    Task<CustomerDetailViewModel?> GetDetailsAsync(int id);
    Task<bool> ExistsRelatedDataAsync(int id);
    Task AddAsync(User customer);
    Task UpdateAsync(User customer);
    Task DeleteAsync(User customer);
}
